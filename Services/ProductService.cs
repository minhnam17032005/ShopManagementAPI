using Demo_Course_Management.Data;
using Demo_Course_Management.DTOs.request;
using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.Middleware;
using Demo_Course_Management.Models;
using Demo_Course_Management.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Demo_Course_Management.Services
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
            // 1. check category tồn tại
            var category = await _repoProduct.GetCategoryByIdAsync(dto.CategoryId)
                ?? throw new NotFoundException("Category not found");

            if (!category.IsActive)
                throw new BadRequestException("Category is inactive");

            // 2. check trùng product trong category
            if (await _repoProduct.ExistsInCategoryAsync(dto.Name, dto.CategoryId))
                throw new BadRequestException("Product already exists in this category");

            // 3. tạo product
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
            var product = await _repoProduct.GetByIdAsync(id)
                ?? throw new NotFoundException("Product not found");

            if (!product.IsActive)
                throw new BadRequestException("Product is inactive");

            var category = await _repoProduct.GetCategoryByIdAsync(dto.CategoryId)
                ?? throw new NotFoundException("Category not found");

            if (!category.IsActive)
                throw new BadRequestException("Category is inactive");

            if (await _repoProduct.ExistsInCategoryExcludeIdAsync(
                id,
                dto.Name,
                dto.CategoryId))
            {
                throw new BadRequestException(
                    "Product already exists in this category");
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
            var product = await _repoProduct.GetByIdWithCategoryAsync(id)
                ?? throw new NotFoundException("Product not found");

            if (!product.IsActive)
                throw new BadRequestException("Product is inactive");

            if (dto.Stock < 0)
                throw new BadRequestException("Stock must be >= 0");

            product.Stock = dto.Stock;
            product.UpdatedAt = DateTime.Now;

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
                ?? throw new NotFoundException("Product not found");

            return MapToDTO(product);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _repoProduct.GetByIdAsync(id)
                ?? throw new NotFoundException("Product not found");

            // đã ngừng hoạt động rồi
            if (!product.IsActive)
                throw new BadRequestException("Sản phẩm đã ngừng hoạt động.");

            // đang nằm trong đơn pending thì không cho ẩn
            var inPendingOrder = await _repoOrderItem.ExistsInPendingOrderAsync(id);

            if (inPendingOrder)
                throw new ConflictException(
                    "Sản phẩm đang nằm trong đơn hàng chờ xử lý nên không thể ngừng hoạt động.");

            // soft delete
            product.IsActive = false;
            product.UpdatedAt = DateTime.Now;

            await _repoProduct.SaveAsync();

            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var product = await _repoProduct.GetByIdAsync(id)
                ?? throw new NotFoundException("Product not found");

            // đã active rồi
            if (product.IsActive)
                throw new BadRequestException("Sản phẩm đang hoạt động.");

            // category bị khóa thì không restore được
            var category = await _repoProduct.GetCategoryByIdAsync(product.CategoryId)
                ?? throw new NotFoundException("Category not found");

            if (!category.IsActive)
                throw new ConflictException(
                    "Danh mục đang ngừng hoạt động nên không thể khôi phục sản phẩm.");

            product.IsActive = true;
            product.UpdatedAt = DateTime.Now;

            await _repoProduct.SaveAsync();

            return true;
        }

        //MAPTODTO RIÊNG
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
                //Lấy từ navigation property (p.Category)
                //Cần Include(p => p.Category) nếu không sẽ bị null
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
                //Lấy trực tiếp từ parameter
                CategoryName = c.Name,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }
    }
}
