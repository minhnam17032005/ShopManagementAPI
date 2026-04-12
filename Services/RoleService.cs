using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.DTOs;
using Microsoft.EntityFrameworkCore;
using Demo_Course_Management.Data;
using Demo_Course_Management.Middleware;
using Demo_Course_Management.Models.Enum;
using Demo_Course_Management.Models;
using Demo_Course_Management.Repositories;

namespace Demo_Course_Management.Services
{
    public class RoleService
    {
        private readonly RoleRepository _repoRole;
        private readonly RolePermissionRepository _repoRolePermission;
        private readonly PermissionRepository _repoPermission;



        public RoleService(RoleRepository repoRole, RolePermissionRepository repoRolePermission, PermissionRepository repoPermission)
        {
            _repoRole = repoRole;
            _repoRolePermission = repoRolePermission;
            _repoPermission = repoPermission;
        }

        // ================= GET ALL =================
        public async Task<List<RoleResponseDTO>> GetAllAsync()
        {
            var roles = await _repoRole.GetAllWithPermissionsAsync();

            return roles.Select(MapToDTO).ToList();
        }

        // ================= GET BY ID =================
        public async Task<RoleResponseDTO> GetByIdAsync(int id)
        {
            var role = await _repoRole.GetByIdWithPermissionsAsync(id)
                ?? throw new NotFoundException("Role not found");

            return MapToDTO(role);
        }

        // ================= ADD PERMISSIONS =================
        public async Task<RolePermissionResponseDTO> AddPermissionsAsync(int roleId, List<int> permissionIds)
        {
            ValidateInput(permissionIds);

            var role = await _repoRole.FindByIdAsync(roleId)
                ?? throw new NotFoundException("Role not found");

            ValidateRole(role);

            // lọc permission hợp lệ trong DB
            var validIds = await _repoPermission.GetValidPermissionIdsAsync(permissionIds);
            if (!validIds.Any())
                throw new NotFoundException("No valid permissions found");

            // lấy permission đã tồn tại trong role
            var existingIds = await _repoPermission.GetExistingPermissionIdsAsync(roleId, validIds);

            // chỉ lấy permission mới chưa có
            var toAdd = validIds.Except(existingIds).ToList();

            if (!toAdd.Any())
                throw new BadRequestException("No new permissions to add");

            // map sang entity để insert bảng many-to-many
            _repoRolePermission.AddRolePermissions(
                toAdd.Select(id => new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = id
                }).ToList()
            );

            await _repoRole.SaveChangesAsync();
            return new RolePermissionResponseDTO
            {
                ProcessedIds = toAdd
            };
        }

        // ================= REMOVE PERMISSIONS =================
        public async Task<RolePermissionResponseDTO> RemovePermissionsAsync(int roleId, List<int> permissionIds)
        {
            ValidateInput(permissionIds);

            var role = await _repoRole.FindByIdAsync(roleId)
                ?? throw new NotFoundException("Role not found");

            ValidateRole(role);

            // Lấy các bản ghi RolePermission đang tồn tại trong DB
            // (chỉ những cái vừa thuộc roleId + nằm trong danh sách cần xóa)
            var entities = await _repoRolePermission.GetRolePermissionsAsync(roleId, permissionIds);

            // Nếu không có bản ghi nào khớp → không có gì để xóa
            if (!entities.Any())
                throw new BadRequestException("None of the provided permissions exist in this role");

            // Xóa các mapping RolePermission khỏi bảng many-to-many
            _repoRolePermission.RemoveRolePermissions(entities);
            await _repoRole.SaveChangesAsync();

            return new RolePermissionResponseDTO
            {
                ProcessedIds = entities.Select(x => x.PermissionId).ToList()
            };
        }

        // ================= MAPPER =================
        private static RoleResponseDTO MapToDTO(Role r)
        {
            return new RoleResponseDTO
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
            };
        }

        // ================= VALIDATION =================
        // check input rỗng/null
        private static void ValidateInput(List<int> permissionIds)
        {
            if (permissionIds == null || !permissionIds.Any())
                throw new BadRequestException("PermissionIds is empty");
        }
        // check quyền được phép sửa
        private static void ValidateRole(Role role)
        {
            if (role.Name != RoleType.STAFF && role.Name != RoleType.CUSTOMER)
                throw new BadRequestException("Only STAFF and CUSTOMER can be modified");
        }
    }
}
