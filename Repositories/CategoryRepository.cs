using ShopManagementAPI.Data;
using ShopManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopManagementAPI.Repositories
{
    public class CategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Categories
                .AnyAsync(x => x.Name == name);
        }

        // check trùng name nhưng loại trừ chính nó (update)
        public async Task<bool> ExistsByNameExcludeIdAsync(string name, int id)
        {
            return await _context.Categories
                .AnyAsync(x => x.Name == name && x.Id != id);
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public IQueryable<Category> Query()
        {
            return _context.Categories.AsNoTracking();
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public void Update(Category category)
        {
            _context.Categories.Update(category);
        }

        public void Remove(Category category)
        {
            _context.Categories.Remove(category);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
