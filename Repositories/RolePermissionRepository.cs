using ShopManagementAPI.Data;
using ShopManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopManagementAPI.Repositories
{
    public class RolePermissionRepository
    {
        private readonly AppDbContext _context;

        public RolePermissionRepository(AppDbContext context)
        {
            _context = context;
        }
        // thêm hàng loạt quyền  
        public void AddRolePermissions(List<RolePermission> entities)
        {
            _context.RolePermissions.AddRange(entities);
        }

        // xóa hàng loạt quyền 
        public void RemoveRolePermissions(List<RolePermission> entities)
        {
            _context.RolePermissions.RemoveRange(entities);
        }

        // lấy các RolePermission entity để xóa
        public async Task<List<RolePermission>> GetRolePermissionsAsync(int roleId, List<int> permissionIds)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
                .ToListAsync();
        }
    }
}
