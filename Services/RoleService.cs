using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.DTOs;
using Microsoft.EntityFrameworkCore;
using Demo_Course_Management.Data;
using Demo_Course_Management.Middleware;
using Demo_Course_Management.Models.Enum;
using Demo_Course_Management.Models;

namespace Demo_Course_Management.Services
{
    public class RoleService
    {
        private readonly AppDbContext _context;

        public RoleService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<RoleResponseDTO>> GetAllAsync()
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Select(r => new RoleResponseDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Permissions = r.RolePermissions
                        .Select(rp => new PermissionItemDTO
                        {
                            Id = rp.Permission.Id,
                            Name = rp.Permission.Name
                        }).ToList(),
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<RoleResponseDTO> GetByIdAsync(int id)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                throw new NotFoundException("Role not found");

            return new RoleResponseDTO
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                Permissions = role.RolePermissions
                    .Select(rp => new PermissionItemDTO
                    {
                        Id = rp.Permission.Id,
                        Name = rp.Permission.Name
                    }).ToList(),
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            };
        }

        public async Task<RolePermissionResponseDTO> AddPermissionsAsync(int roleId, List<int> permissionIds)
        {
            if (permissionIds == null || !permissionIds.Any())
                throw new BadRequestException("PermissionIds is empty");

            var role = await _context.Roles.FindAsync(roleId)
                ?? throw new NotFoundException("Role not found");

            if (role.Name != RoleType.STAFF && role.Name != RoleType.CUSTOMER)
                throw new BadRequestException("Only STAFF and CUSTOMER can be modified");

            // lấy permission hợp lệ + chưa tồn tại trong role luôn (1 query logic)
            var toAdd = await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id))
                .Where(p => !_context.RolePermissions
                    .Any(rp => rp.RoleId == roleId && rp.PermissionId == p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            if (!toAdd.Any())
                throw new BadRequestException("No new permissions to add");

            _context.RolePermissions.AddRange(
                toAdd.Select(id => new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = id
                })
            );
            await _context.SaveChangesAsync();
            return new RolePermissionResponseDTO
            {
                ProcessedIds = toAdd
            };
        }

        public async Task<RolePermissionResponseDTO> RemovePermissionsAsync(int roleId, List<int> permissionIds)
        {
            if (permissionIds == null || !permissionIds.Any())
                throw new BadRequestException("PermissionIds is empty");

            var role = await _context.Roles.FindAsync(roleId)
                ?? throw new NotFoundException("Role not found");

            if (role.Name != RoleType.STAFF && role.Name != RoleType.CUSTOMER)
                throw new BadRequestException("Only STAFF and CUSTOMER can be modified");

            // Lấy danh sách permission hiện đang gán trong role và nằm trong danh sách cần xóa
            var entities = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
                .ToListAsync();

            if (!entities.Any())
                throw new BadRequestException("None of the provided permissions exist in this role, so nothing was removed");

            _context.RolePermissions.RemoveRange(entities);

            await _context.SaveChangesAsync();

            return new RolePermissionResponseDTO
            {
                ProcessedIds = entities.Select(x => x.PermissionId).ToList()
            };
        }

    }
}
