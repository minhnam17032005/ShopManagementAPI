using Demo_Course_Management.Data;
using Demo_Course_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_Course_Management.Repositories
{
    public class UserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsUsernameExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.Username == username);
        }

        public async Task<bool> IsEmailExists(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email == email);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(x => x.Role)
                .ToListAsync();
        }

        public async Task<User?> GetByIdWithRoleAsync(int id)
        {
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
        }

    }
}
