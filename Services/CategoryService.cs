using Demo_Course_Management.Data;
using Demo_Course_Management.DTOs.request;
using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.Middleware;
using Demo_Course_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_Course_Management.Services
{
    public class CategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<CategoryResponseDTO> CreateAsync(CategoryRequestDTO dto)
        {
            // check trùng name
            if (await _context.Categories.AnyAsync(x => x.Name == dto.Name))
            {
                throw new BadRequestException("Category name already exists");
            }

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryResponseDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        public async Task<CategoryResponseDTO> UpdateAsync(int id, CategoryRequestDTO dto)
        {
            // 1. Tìm category
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                throw new NotFoundException("Category not found");
            }

            // 2. Check trùng name (trừ chính nó)
            if (await _context.Categories
                .AnyAsync(x => x.Name == dto.Name && x.Id != id))
            {
                throw new BadRequestException("Category name already exists");
            }

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new CategoryResponseDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
        public async Task<List<CategoryResponseDTO>> GetAllAsync()
        {
            var categories = await _context.Categories.ToListAsync();

            return categories.Select(c => new CategoryResponseDTO
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();
        }

        public async Task<CategoryResponseDTO> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                throw new NotFoundException("Category not found");
            }   

            return new CategoryResponseDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                throw new NotFoundException("Category not found");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return true;
        }




    }
}
