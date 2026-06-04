using ShopManagementAPI.Data;
using ShopManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopManagementAPI.Repositories
{
    public class PermissionRepository
    {
        private readonly AppDbContext _context;

        public PermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Permission>> GetAllAsync()
        {
            return await _context.Permissions.ToListAsync();
        }

        public async Task<Permission?> GetByIdAsync(int id)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // lấy danh sách permission tồn tại trong db
        public async Task<List<int>> GetValidPermissionIdsAsync(List<int> permissionIds)
        {
            return await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();
        }
        // lấy danh sách permission đã gán cho role
        public async Task<List<int>> GetExistingPermissionIdsAsync(int roleId, List<int> permissionIds)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
                .Select(rp => rp.PermissionId)
                .ToListAsync();
        }

    }
}
