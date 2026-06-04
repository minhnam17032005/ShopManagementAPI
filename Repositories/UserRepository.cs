using ShopManagementAPI.Data;
using ShopManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopManagementAPI.Repositories
{
    public class UserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        //filter, search, sort, paging
        public IQueryable<User> Query()
        {
            return _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role);
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
                .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                .ToListAsync();
        }

        public async Task<User?> GetByIdWithRolesAsync(int id)
        {
            return await _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
        }
        // lấy user + roles để xử lý đăng nhập
        public async Task<User?> GetByUsernameWithRolesAsync(string username)
        {
            return await _context.Users
                .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Username == username);
        }

        //lấy name của all permissions 
        public async Task<List<string>> GetPermissionNamesAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToListAsync();
        }

        // lấy user + roles từ refresh token để cấp access token mới
        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
        }

        // lấy user kèm toàn bộ quyền của user
        public async Task<User?> GetUserWithRolesAndPermissionsAsync(int userId)
        {
            return await _context.Users
                .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                        .ThenInclude(x => x.RolePermissions)
                            .ThenInclude(x => x.Permission)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }



    }
}
