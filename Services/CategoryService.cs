using ShopManagementAPI.Data;
using ShopManagementAPI.DTOs.request;
using ShopManagementAPI.DTOs.response;
using ShopManagementAPI.Exceptions;
using ShopManagementAPI.Models;
using ShopManagementAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ShopManagementAPI.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository _repoCategory;
        private readonly ProductRepository _repoProduct;

        public CategoryService(CategoryRepository repoCategory, ProductRepository repoProduct)
        {
            _repoCategory = repoCategory;
            _repoProduct = repoProduct;
        }

        public async Task<CategoryResponseDTO> CreateAsync(CategoryRequestDTO dto)
        {
            // 1. check trùng name
            if (await _repoCategory.ExistsByNameAsync(dto.Name))
                throw new ConflictException("Category name already exists");

            // 2. tạo entity
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            // 3. lưu DB
            await _repoCategory.AddAsync(category);
            await _repoCategory.SaveChangesAsync();

            // 4. trả về DTO
            return MapToDTO(category);
        }

        public async Task<CategoryResponseDTO> UpdateAsync(int id, CategoryRequestDTO dto)
        {
            // 1. tìm category
            var category = await _repoCategory.GetByIdAsync(id)
                ?? throw new NotFoundException("Category not found");
            if (!category.IsActive)
                throw new ConflictException("Danh mục đã ngừng hoạt động.");

            // 2. check trùng name (trừ chính nó)
            if (await _repoCategory.ExistsByNameExcludeIdAsync(dto.Name, id))
                throw new ConflictException("Category name already exists");

            // 3. update dữ liệu
            category.Name = dto.Name;
            category.Description = dto.Description;
            category.UpdatedAt = DateTime.UtcNow;

            // 4. lưu DB
            _repoCategory.Update(category);
            await _repoCategory.SaveChangesAsync();

            return MapToDTO(category);
        }

        public async Task<List<CategoryResponseDTO>> GetAllAsync()
        {
            var categories = await _repoCategory.GetAllAsync();

            return categories
                .Select(MapToDTO)
                .ToList();
        }

        public async Task<CategoryResponseDTO> GetByIdAsync(int id)
        {
            var category = await _repoCategory.GetByIdAsync(id)
                ?? throw new NotFoundException("Category not found");
            if (!category.IsActive)
                throw new NotFoundException("Category not found");

            return MapToDTO(category);
        }

        public async Task<StatusResponseDTO> DeleteAsync(int id)
        {
            var category = await _repoCategory.GetByIdAsync(id)
                ?? throw new NotFoundException("Category not found");

            var hasProducts = await _repoProduct.HasActiveProductsAsync(id);

            if (hasProducts)
                throw new ConflictException(
                    "Danh mục đang chứa sản phẩm hoạt động nên không thể ngừng hoạt động.");

            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;

            await _repoCategory.SaveChangesAsync();

            return new StatusResponseDTO
            {
                IsActive = category.IsActive
            };
        }

        public async Task<StatusResponseDTO> RestoreAsync(int id)
        {
            var category = await _repoCategory.GetByIdAsync(id)
                ?? throw new NotFoundException("Category not found");

            if (category.IsActive)
                throw new ConflictException("Danh mục đang hoạt động.");

            category.IsActive = true;
            category.UpdatedAt = DateTime.UtcNow;

            await _repoCategory.SaveChangesAsync();

            return new StatusResponseDTO
            {
                IsActive = category.IsActive
            };
        }

        // helper mapping
        private static CategoryResponseDTO MapToDTO(Category c)
        {
            return new CategoryResponseDTO
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive =c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }


    }
}
