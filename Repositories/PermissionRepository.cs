using Demo_Course_Management.Data;
using Demo_Course_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_Course_Management.Repositories
{
    public class PermissionRepository
    {
        private readonly AppDbContext _context;

        public PermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        // lấy tất cả permission (raw entity)
        public async Task<List<Permission>> GetAllAsync()
        {
            return await _context.Permissions.ToListAsync();
        }

        // lấy theo id
        public async Task<Permission?> GetByIdAsync(int id)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // Lấy danh sách permission hợp lệ (tồn tại trong DB)
        public async Task<List<int>> GetValidPermissionIdsAsync(List<int> permissionIds)
        {
            return await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();
        }
        // Lấy danh sách permission đã tồn tại trong role
        public async Task<List<int>> GetExistingPermissionIdsAsync(int roleId, List<int> permissionIds)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
                .Select(rp => rp.PermissionId)
                .ToListAsync();
        }

    }
}
