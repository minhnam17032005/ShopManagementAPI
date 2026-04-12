using Demo_Course_Management.Data;
using Demo_Course_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_Course_Management.Repositories
{
    public class RoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Role>> GetAllWithPermissionsAsync()
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<Role?> GetByIdWithPermissionsAsync(int id)
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role?> FindByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
