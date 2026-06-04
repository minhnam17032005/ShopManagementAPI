using ShopManagementAPI.Data;
using ShopManagementAPI.DTOs.request;
using ShopManagementAPI.DTOs.response;
using ShopManagementAPI.Exceptions;
using ShopManagementAPI.Models;
using ShopManagementAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ShopManagementAPI.Services
{
    public class ProductService
    {
        private readonly ProductRepository _repoProduct;
        private readonly OrderItemRepository _repoOrderItem;

        public ProductService(ProductRepository repoProduct, OrderItemRepository repoOrderItem)
        {
            _repoProduct = repoProduct;
            _repoOrderItem = repoOrderItem;
        }

        public async Task<ProductResponseDTO> CreateAsync(ProductRequestDTO dto)
        {
            // kiểm tra category tồn tại
            var category = await _repoProduct.GetCategoryByIdAsync(dto.CategoryId)
                ?? throw new NotFoundException("Không tìm thấy danh mục");

            // kiểm tra category đang hoạt động
            if (!category.IsActive)
                throw new ConflictException("Danh mục đã ngừng hoạt động");

            // kiểm tra trùng sản phẩm trong danh mục
            if (await _repoProduct.ExistsInCategoryAsync(dto.Name, dto.CategoryId))
                throw new ConflictException("Sản phẩm đã tồn tại trong danh mục này");

            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId
            };

            await _repoProduct.AddAsync(product);
            await _repoProduct.SaveAsync();

            return MapToDTO(product, category);
        }

        public async Task<ProductResponseDTO> UpdateAsync(
        int id,
        UpdateProductDTO dto)
        {
            // kiểm tra product tồn tại
            var product = await _repoProduct.GetByIdAsync(id)
                ?? throw new NotFoundException("Không tìm thấy sản phẩm");

            // kiểm tra trạng thái product
            if (!product.IsActive)
                throw new ConflictException("Sản phẩm đã ngừng hoạt động");

            // kiểm tra category tồn tại
            var category = await _repoProduct.GetCategoryByIdAsync(dto.CategoryId)
                ?? throw new NotFoundException("Không tìm thấy danh mục");

            // kiểm tra category hoạt động
            if (!category.IsActive)
                throw new ConflictException("Danh mục đã ngừng hoạt động");

            // kiểm tra trùng product trong category (loại trừ chính nó)
            if (await _repoProduct.ExistsInCategoryExcludeIdAsync(
                id,
                dto.Name,
                dto.CategoryId))
            {
                throw new ConflictException("Sản phẩm đã tồn tại trong danh mục này");
            }

            product.Name = dto.Name;
            product.Price = dto.Price;
            product.CategoryId = dto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            await _repoProduct.SaveAsync();

            return MapToDTO(product, category);
        }

        public async Task<ProductResponseDTO> UpdateStockAsync(int id,UpdateStockDTO dto)
        {
            // kiểm tra product tồn tại
            var product = await _repoProduct.GetByIdWithCategoryAsync(id)
                ?? throw new NotFoundException("Không tìm thấy sản phẩm");

            // kiểm tra trạng thái product
            if (!product.IsActive)
                throw new ConflictException("Sản phẩm đã ngừng hoạt động");

            // kiểm tra stock hợp lệ
            if (dto.Stock < 0)
                throw new BadRequestException("Số lượng tồn kho không được nhỏ hơn 0");

            // cập nhật stock
            product.Stock = dto.Stock;
            product.UpdatedAt = DateTime.UtcNow;
            await _repoProduct.SaveAsync();

            return MapToDTO(product);
        }

        public async Task<List<ProductResponseDTO>> GetAllAsync()
        {
            var products = await _repoProduct.GetAllWithCategoryAsync();

            return products.Select(MapToDTO).ToList();
        }

        public async Task<ProductResponseDTO> GetByIdAsync(int id)
        {
            var product = await _repoProduct.GetByIdWithCategoryAsync(id)
                ?? throw new NotFoundException("Không tìm thấy sản phâm");

            return MapToDTO(product);
        }

        public async Task<StatusResponseDTO> DeleteAsync(int id)
        {
            var product = await _repoProduct.GetByIdAsync(id)
                ?? throw new NotFoundException("Không tìm thấy sản phâm");

            // kiểm tra đã inactive chưa
            if (!product.IsActive)
                throw new ConflictException("Sản phẩm đã ngừng hoạt động.");

            // kiểm tra có trong đơn pending không
            var inPendingOrder = await _repoOrderItem.ExistsInPendingOrderAsync(id);

            if (inPendingOrder)
                throw new ConflictException(
                    "Sản phẩm đang nằm trong đơn hàng chờ xử lý nên không thể ngừng hoạt động.");

            // soft delete (disable product)
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _repoProduct.SaveAsync();

            return new StatusResponseDTO
            {
                IsActive = product.IsActive
            };
        }

        public async Task<StatusResponseDTO> RestoreAsync(int id)
        {
            var product = await _repoProduct.GetByIdAsync(id)
                ?? throw new NotFoundException("Không tìm thấy sản phâm");

            // kiểm tra đã active chưa
            if (product.IsActive)
                throw new ConflictException("Sản phẩm đang hoạt động.");

            // kiểm tra category
            var category = await _repoProduct.GetCategoryByIdAsync(product.CategoryId)
                ?? throw new NotFoundException("Không tìm thấy danh mục");

            // category phải active
            if (!category.IsActive)
                throw new ConflictException(
                    "Danh mục đang ngừng hoạt động nên không thể khôi phục sản phẩm.");

            product.IsActive = true;
            product.UpdatedAt = DateTime.UtcNow;

            await _repoProduct.SaveAsync();

            return new StatusResponseDTO
            {
                IsActive = product.IsActive
            };
        }

        private static ProductResponseDTO MapToDTO(Product p)
        {
            return new ProductResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }

        // overload cho create/update (có category riêng)
        private static ProductResponseDTO MapToDTO(Product p, Category c)
        {
            return new ProductResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                IsActive =p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = c.Name,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }
    }
}
