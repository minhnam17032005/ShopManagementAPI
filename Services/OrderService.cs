using Demo_Course_Management.DTOs;
using Demo_Course_Management.DTOs.request;
using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.Middleware;
using Demo_Course_Management.Models;
using Demo_Course_Management.Models.Enum;
using Demo_Course_Management.Repositories;

namespace Demo_Course_Management.Services
{
    public class OrderService
    {
        private readonly OrderRepository _repoOrder;
        private readonly OrderItemRepository _repoOrderItem;
        private readonly ProductRepository _repoProduct;
        private readonly UserRepository _repoUser;



        public OrderService(OrderRepository repoOrder, OrderItemRepository repoOrderItem, ProductRepository repoProduct, UserRepository repoUser)
        {
            _repoOrder = repoOrder;
            _repoOrderItem = repoOrderItem;
            _repoProduct = repoProduct;
            _repoUser = repoUser;
        }

        public async Task<OrderResponseDTO> CreateAsync(CreateOrderReqDTO dto)
        {
            // 1. Check user tồn tại
            var user = await _repoUser.GetByIdAsync(dto.UserId);
            if (user == null)
                throw new NotFoundException($"Không tìm thấy User ID = {dto.UserId}");

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
                    UserId = dto.UserId,
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

        public async Task<OrderResponseDTO> UpdateStatusAsync(int id,UpdateOrderStatusReqDTO dto)
        {
            var order = await _repoOrder.GetByIdAsync(id);

            if (order == null)
                throw new NotFoundException("Không tìm thấy đơn hàng.");

            // Chỉ cho phép đổi từ Pending
            if (order.Status == OrderStatus.COMPLETED)
                throw new BadRequestException(
                    "Đơn hàng đã hoàn thành, không thể thay đổi trạng thái.");

            if (order.Status == OrderStatus.CANCELLED)
                throw new BadRequestException(
                    "Đơn hàng đã hủy, không thể thay đổi trạng thái.");

            // Chỉ cho Pending -> Completed hoặc Pending -> Cancelled
            if (dto.Status != OrderStatus.COMPLETED &&
                dto.Status != OrderStatus.CANCELLED)
            {
                throw new BadRequestException(
                    "Chỉ được chuyển sang Completed hoặc Cancelled.");
            }
            // Pending -> Cancelled => hoàn kho
            if (dto.Status == OrderStatus.CANCELLED)
            {
                foreach (var item in order.OrderItems)
                {
                    var product = await _repoProduct.GetByIdAsync(item.ProductId);
                    if (product != null)
                        product.Stock += item.Quantity;
                }

                await _repoProduct.SaveAsync();
            }
            // Pending -> Completed
            // Không cộng/trừ kho vì đã trừ từ lúc tạo đơn

            order.Status = dto.Status;
            order.UpdatedAt = DateTime.Now;

            await _repoOrder.SaveAsync();

            return MapToDTO(order);
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

        private OrderResponseDTO MapToDTO(Order order, List<OrderItemResponseDTO> items)
        {
            return new OrderResponseDTO
            {
                Id = order.Id,
                UserId = order.UserId,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Items = items,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }
        private OrderResponseDTO MapToDTO(Order order)
        {
            return new OrderResponseDTO
            {
                Id = order.Id,
                UserId = order.UserId,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,

                Items = order.OrderItems.Select(x => new OrderItemResponseDTO
                {
                    ProductId = x.ProductId,
                    Name = x.Product.Name,
                    Quantity = x.Quantity,
                    Price = x.Price
                }).ToList()
            };
        }
    }
} 
