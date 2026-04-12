using Demo_Course_Management.Data;
using Demo_Course_Management.DTOs.request;
using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.Middleware;
using Demo_Course_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_Course_Management.Services
{
    public class ProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ProductResponseDTO> CreateAsync(ProductRequestDTO dto)
        {
            var category = await _context.Categories
                .Where(c => c.Id == dto.CategoryId)
                .Select(c => new { c.Id, c.Name })
                .FirstOrDefaultAsync();//data acess nên tách ra viết ở repo

            if (category == null)
            {
                throw new NotFoundException("Category not found");
            }

            // check trùng 
            if (await _context.Products
                .AnyAsync(p => p.Name == dto.Name && p.CategoryId == dto.CategoryId))//data acess nên tách ra viết ở repo
            {
                throw new BadRequestException("Product already exists in this category");
            }

            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId
            };

            _context.Products.Add(product);//data acess nên tách ra viết ở repo
            await _context.SaveChangesAsync();

            return new ProductResponseDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                CategoryName = category.Name,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        public async Task<ProductResponseDTO> UpdateAsync(int id, ProductRequestDTO dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                throw new NotFoundException("Product not found");
            }

            var category = await _context.Categories
                .Where(c => c.Id == dto.CategoryId)
                .Select(c => new { c.Id, c.Name })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                throw new NotFoundException("Category not found");
            }

            // check trùng
            if (await _context.Products
                .AnyAsync(p => p.Id != id
                            && p.CategoryId == dto.CategoryId
                            && p.Name == dto.Name))
            {
                throw new BadRequestException("Product already exists in this category");
            }

            product.Name = dto.Name;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.CategoryId = dto.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ProductResponseDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                CategoryName = category.Name,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        public async Task<List<ProductResponseDTO>> GetAllAsync()
        {
            return await _context.Products
                .Join(_context.Categories,
                    p => p.CategoryId,
                    c => c.Id,
                    (p, c) => new ProductResponseDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        Stock = p.Stock,
                        CategoryId = p.CategoryId,
                        CategoryName = c.Name,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ProductResponseDTO> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id)
                .Join(_context.Categories,
                    p => p.CategoryId,
                    c => c.Id,
                    (p, c) => new ProductResponseDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        Stock = p.Stock,
                        CategoryId = p.CategoryId,
                        CategoryName = c.Name,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                throw new NotFoundException("Product not found");
            }

            return product;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                throw new NotFoundException("Product not found");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
