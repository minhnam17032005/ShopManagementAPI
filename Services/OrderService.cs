using ShopManagementAPI.DTOs;
using ShopManagementAPI.DTOs.request;
using ShopManagementAPI.DTOs.response;
using ShopManagementAPI.Jwt;
using ShopManagementAPI.Exceptions;
using ShopManagementAPI.Models;
using ShopManagementAPI.Models.Enum;
using ShopManagementAPI.Repositories;

namespace ShopManagementAPI.Services
{
    public class OrderService
    {
        private readonly OrderRepository _repoOrder;
        private readonly OrderItemRepository _repoOrderItem;
        private readonly ProductRepository _repoProduct;
        private readonly UserRepository _repoUser;
        private readonly CurrentUserService _currentUser;

        private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> AllowedTransitions = new()
            {
                [OrderStatus.PENDING] = new HashSet<OrderStatus>{OrderStatus.CONFIRMED,OrderStatus.CANCELLED}
                ,

                [OrderStatus.CONFIRMED] = new HashSet<OrderStatus>{OrderStatus.SHIPPING,OrderStatus.CANCELLED}
                ,

                [OrderStatus.SHIPPING] = new HashSet<OrderStatus>{OrderStatus.COMPLETED,OrderStatus.RETURNED}
                ,

                [OrderStatus.COMPLETED] = new HashSet<OrderStatus>(),
                [OrderStatus.CANCELLED] = new HashSet<OrderStatus>(),   
                [OrderStatus.RETURNED] = new HashSet<OrderStatus>(),
            };

        public OrderService(OrderRepository repoOrder, OrderItemRepository repoOrderItem, 
            ProductRepository repoProduct, UserRepository repoUser, CurrentUserService currentUser)
        {
            _repoOrder = repoOrder;
            _repoOrderItem = repoOrderItem;
            _repoProduct = repoProduct;
            _repoUser = repoUser;
            _currentUser = currentUser;
        }

        public async Task<OrderResponseDTO> CreateAsync(CreateOrderReqDTO dto)
        {
            // 2. Check có sản phẩm
            if (dto.Items == null || !dto.Items.Any())
                throw new BadRequestException("Đơn hàng phải có ít nhất 1 sản phẩm.");

            // 3. Bắt đầu transaction
            using var tran = await _repoOrder.BeginTransactionAsync();

            try
            {
                decimal totalAmount = 0;
                var errors = new List<string>();
                var itemResponses = new List<OrderItemResponseDTO>();

                // 4. Lấy toàn bộ product 1 lần
                var productIds = dto.Items
                    .Select(x => x.ProductId)
                    .Distinct()
                    .ToList();

                var products = await _repoProduct.GetByIdsAsync(productIds);

                // convert thành dictionary
                var productMap = products.ToDictionary(x => x.Id);

                // 5. Validate + trừ stock
                foreach (var item in dto.Items)
                {
                    if (!productMap.ContainsKey(item.ProductId))
                    {
                        errors.Add($"Không tìm thấy sản phẩm ID = {item.ProductId}");
                        continue;
                    }
                    var product = productMap[item.ProductId];
                    if (!product.IsActive)
                    {
                        errors.Add($"Sản phẩm {product.Name} đã ngừng hoạt động.");
                        continue;
                    }
                    if (item.Quantity <= 0)
                    {
                        errors.Add($"Số lượng sản phẩm {product.Name} phải > 0.");
                        continue;
                    }
                    if (product.Stock < item.Quantity)
                    {
                        errors.Add($"Sản phẩm {product.Name} không đủ hàng.");
                        continue;
                    }

                    // trừ kho
                    product.Stock -= item.Quantity;

                    decimal lineTotal = product.Price * item.Quantity;
                    totalAmount += lineTotal;

                    itemResponses.Add(new OrderItemResponseDTO
                    {
                        ProductId = product.Id,
                        Name = product.Name,
                        Quantity = item.Quantity,
                        Price = product.Price
                    });
                }
                // có lỗi -> rollback
                if (errors.Any())
                    throw new BadRequestException(errors);

                var order = new Order
                {
                    UserId = _currentUser.UserId,

                    ReceiverName = dto.ReceiverName,
                    PhoneNumber = dto.PhoneNumber,
                    ShippingAddress = dto.ShippingAddress,
                    Note = dto.Note,
                    Status = OrderStatus.PENDING,
                    TotalAmount = totalAmount
                };

                await _repoOrder.AddAsync(order);
                await _repoOrder.SaveAsync();

                foreach (var item in itemResponses)
                {
                    await _repoOrderItem.AddAsync(new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });
                }

                await _repoOrderItem.SaveAsync();
                // 8. Save stock
                await _repoProduct.SaveAsync();
                // 9. Commit
                await tran.CommitAsync();
                return MapToDTO(order, itemResponses);
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderResponseDTO> UpdateStatusAsync(int id, UpdateOrderStatusReqDTO dto)
        {
            using var tran = await _repoOrder.BeginTransactionAsync();

            try
            {
                var order = await _repoOrder.GetByIdAsync(id);

                if (order == null)
                    throw new NotFoundException("Không tìm thấy đơn hàng.");

                if (!Enum.IsDefined(typeof(OrderStatus), dto.Status))
                    throw new BadRequestException("Trạng thái hơn hàng không hợp lệ");

                // 1. validate FSM
                ValidateTransition(order.Status, dto.Status);

                // 2. xử lý business theo status
                if (dto.Status == OrderStatus.CANCELLED)
                {
                    await RestockAsync(order);
                }

                if (dto.Status == OrderStatus.RETURNED)
                {
                    await RestockAsync(order);
                }

                // 3. update order
                order.Status = dto.Status;
                order.UpdatedAt = DateTime.UtcNow;

                await _repoOrder.SaveAsync();

                await tran.CommitAsync();

                return MapToDTO(order);
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        public async Task<List<OrderResponseDTO>> GetAllAsync()
        {
            var orders = await _repoOrder.GetAllAsync();

            return orders.Select(MapToDTO).ToList();
        }

        public async Task<OrderResponseDTO> GetByIdAsync(int id)
        {
            var order = await _repoOrder.GetByIdAsync(id);

            if (order == null)
                throw new NotFoundException("Không tìm thấy đơn hàng.");

            return MapToDTO(order);
        }
        public async Task<List<OrderResponseDTO>> GetMyOrdersAsync()
        {
            var userId = _currentUser.UserId;

            var orders = await _repoOrder
                .GetByUserIdAsync(userId);

            return orders
                .Select(MapToDTO)
                .ToList();
        }

        public async Task<OrderResponseDTO> GetMyOrderByIdAsync(int id)
        {
            var userId = _currentUser.UserId;

            var order = await _repoOrder
                .GetByIdAndUserIdAsync(id, userId);

            if (order == null)
                throw new NotFoundException(
                    "Không tìm thấy đơn hàng.");

            return MapToDTO(order);
        }
        
        //restock nếu như đơn hàng có trạng thái CANCELLED và RETURNED
        private async Task RestockAsync(Order order)
        {
            var items = order.OrderItems;

            if (items == null || !items.Any())
                return;

            var productIds = items
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

            // Query tất cả product liên quan 1 lần
            var products = await _repoProduct.GetByIdsAsync(productIds);

            // Convert sang Dictionary để lookup nhanh
            var map = products.ToDictionary(x => x.Id);

            // Duyệt từng item trong đơn hàng để hoàn kho
            foreach (var item in items)
            {
                // Kiểm tra product có tồn tại trong map không
                if (map.TryGetValue(item.ProductId, out var product))
                {
                    product.Stock += item.Quantity;
                }
            }

            await _repoProduct.SaveAsync();
        }

        public async Task<OrderResponseDTO> CancelOrderAsync(int id)
        {
            using var tran = await _repoOrder.BeginTransactionAsync();
            try
            {
                var currentUserId = _currentUser.UserId;

                var order = await _repoOrder
                    .GetByIdAndUserIdAsync(id, currentUserId);

                if (order == null)
                    throw new NotFoundException(
                        "Không tìm thấy đơn hàng.");

                if (order.Status != OrderStatus.PENDING)
                {
                    throw new BadRequestException(
                        "Chỉ đơn hàng đang chờ xác nhận mới được hủy.");
                }

                await RestockAsync(order);

                order.Status = OrderStatus.CANCELLED;
                order.UpdatedAt = DateTime.UtcNow;

                await _repoOrder.SaveAsync();

                await tran.CommitAsync();

                return MapToDTO(order);
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        //check trạng thái tiếp theo xem đã hợp lí 
        private void ValidateTransition(OrderStatus current, OrderStatus next)
        {
            if (!AllowedTransitions.TryGetValue(current, out var allowed))
                throw new ConflictException("Trạng thái hiện tại không hợp lệ.");

            if (!allowed.Contains(next))
                throw new ConflictException($"Không thể chuyển từ {current} sang {next}");
        }
        private OrderResponseDTO MapToDTO(Order order,List<OrderItemResponseDTO> items)
        {
            return new OrderResponseDTO
            {
                Id = order.Id,
                UserId = order.UserId,

                ReceiverName = order.ReceiverName,
                PhoneNumber = order.PhoneNumber,
                ShippingAddress = order.ShippingAddress,
                Note = order.Note,

                Status = order.Status,
                TotalAmount = order.TotalAmount,

                Items = items,

                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }

        private OrderResponseDTO MapToDTO(Order order)
        {
            return MapToDTO(
                order,
                order.OrderItems.Select(x => new OrderItemResponseDTO
                {
                    ProductId = x.ProductId,
                    Name = x.Product.Name,
                    Quantity = x.Quantity,
                    Price = x.Price
                }).ToList()
            );
        }
    }
} 
