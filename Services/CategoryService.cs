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
            // kiểm tra trùng tên category
            if (await _repoCategory.ExistsByNameAsync(dto.Name))
                throw new ConflictException("Tên danh mục đã tồn tại");

            // tạo category mới
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            // lưu database
            await _repoCategory.AddAsync(category);
            await _repoCategory.SaveChangesAsync();

            return MapToDTO(category);
        }

        public async Task<CategoryResponseDTO> UpdateAsync(int id, CategoryRequestDTO dto)
        {
            // tìm category theo id
            var category = await _repoCategory.GetByIdAsync(id)
                ?? throw new NotFoundException("Không tìm thấy danh mục");
            if (!category.IsActive)
                throw new ConflictException("Danh mục đã ngừng hoạt động.");

            // kiểm tra trạng thái hoạt động
            if (await _repoCategory.ExistsByNameExcludeIdAsync(dto.Name, id))
                throw new ConflictException("Tên danh mục đã tồn tại");

            // kiểm tra trùng tên (loại trừ chính nó)
            category.Name = dto.Name;
            category.Description = dto.Description;
            category.UpdatedAt = DateTime.UtcNow;

            // lưu thay đổi 
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
                ?? throw new NotFoundException("Không tìm thấy danh mục");
            if (!category.IsActive)
                throw new NotFoundException("Không tìm thấy danh mục");

            return MapToDTO(category);
        }

        public async Task<StatusResponseDTO> DeleteAsync(int id)
        {
            var category = await _repoCategory.GetByIdAsync(id)
                ?? throw new NotFoundException("Không tìm thấy danh mục");

            // kiểm tra còn sản phẩm active không
            var hasProducts = await _repoProduct.HasActiveProductsAsync(id);

            if (hasProducts)
                throw new ConflictException(
                    "Danh mục đang chứa sản phẩm hoạt động nên không thể ngừng hoạt động.");

            // soft delete (disable category)
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
                ?? throw new NotFoundException("Không tìm thấy danh mục");

            // nếu đang active rồi
            if (category.IsActive)
                throw new ConflictException("Không tìm thấy danh mục");

            // kích hoạt lại category
            category.IsActive = true;
            category.UpdatedAt = DateTime.UtcNow;

            await _repoCategory.SaveChangesAsync();

            return new StatusResponseDTO
            {
                IsActive = category.IsActive
            };
        }

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
