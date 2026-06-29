using ShopManagementAPI.Data;
using ShopManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopManagementAPI.Repositories
{
    public class ProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        // kiểm tra product đã tồn tại trong cùng category chưa
        public async Task<bool> ExistsInCategoryAsync(string name, int categoryId)
        {
            return await _context.Products
                .AnyAsync(p => p.Name == name && p.CategoryId == categoryId);
        }

        // kiểm tra trùng khi update (loại trừ chính nó)
        public async Task<bool> ExistsInCategoryExcludeIdAsync(int id, string name, int categoryId)
        {
            return await _context.Products
                .AnyAsync(p => p.Id != id && p.Name == name && p.CategoryId == categoryId);
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }
        public async Task<List<Product>> GetByIdsAsync(List<int> ids)
        {
            return await _context.Products
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
        }

        public IQueryable<Product> Query()
        {
            return _context.Products
                .Include(x => x.Category)
                .AsNoTracking();
        }

        public async Task<Product?> GetByIdWithCategoryAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public void Remove(Product product)
        {
            _context.Products.Remove(product);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasActiveProductsAsync(int categoryId)
        {
            return await _context.Products
                .AnyAsync(x =>
                    x.CategoryId == categoryId &&
                    x.IsActive);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Products.CountAsync();
        }
    }
}
