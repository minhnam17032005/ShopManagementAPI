using ShopManagementAPI.DTOs.response;
using ShopManagementAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using ShopManagementAPI.Data;
using ShopManagementAPI.Exceptions;
using ShopManagementAPI.Models.Enum;
using ShopManagementAPI.Models;
using ShopManagementAPI.Repositories;
using ShopManagementAPI.Authorization;
using ShopManagementAPI.Jwt;
using static ShopManagementAPI.Services.RoleService;
using ShopManagementAPI.DTOs.response.Role;

namespace ShopManagementAPI.Services
{
    public class RoleService
    {
        private readonly RoleRepository _repoRole;
        private readonly RolePermissionRepository _repoRolePermission;
        private readonly PermissionRepository _repoPermission;
        private readonly PermissionCacheService _permissionCacheService;
        private readonly CurrentUserService _currentUserService;
        private readonly UserDataScopeService _userDataScopeService;



        public RoleService(RoleRepository repoRole, RolePermissionRepository repoRolePermission, CurrentUserService currentUserService,
                    PermissionRepository repoPermission, PermissionCacheService permissionCacheService, UserDataScopeService userDataScopeService)
        {
            _repoRole = repoRole;
            _repoRolePermission = repoRolePermission;
            _repoPermission = repoPermission;
            _permissionCacheService = permissionCacheService;
            _currentUserService = currentUserService;
            _userDataScopeService = userDataScopeService;
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
                ?? throw new NotFoundException("Không tìm thấy vai trò");

            return MapToDTO(role);
        }

        // ================= ADD PERMISSIONS =================
        public async Task<RolePermissionResponseDTO> AddPermissionsAsync(
        int roleId,
        List<int> permissionIds)
        {
            ValidateInput(permissionIds);

            var role = await _repoRole.FindByIdAsync(roleId)
                ?? throw new NotFoundException("Không tìm thấy vai trò");
            ValidateRole(role);

            // ================= DATA SCOPE =================
            var currentRoles = _currentUserService.Roles
                .Select(r => Enum.Parse<RoleType>(r))
                    .ToList();

            if (!_userDataScopeService.CanManageRole(currentRoles,role))
            {
                throw new ForbiddenException(
                    "Không có quyền chỉnh sửa vai trò ADMIN");
            }

            //Lấy permission hợp lệ trong DB
            var validIds =
                await _repoPermission.GetValidPermissionIdsAsync(permissionIds);

            //Lấy permission đã tồn tại trong role
            var existingIds =
                await _repoPermission.GetExistingPermissionIdsAsync(
                    roleId,
                    validIds);

            //Xác định permission cần thêm
            var toAdd = validIds.Except(existingIds).ToList();

            //Xác định permission fail (không tồn tại hoặc đã có)
            var failedIds = permissionIds
                .Except(validIds)
                .Union(existingIds)
                .Distinct()
                .ToList();

            //Nếu không có cái mới nào
            if (!toAdd.Any())
            {
                return new RolePermissionResponseDTO
                {
                    RoleId = roleId,
                    ProcessedIds = new List<int>(),
                    FailedIds = permissionIds,
                    Message = "Không có quyền mới để thêm"
                };
            }

            //Insert mới
            _repoRolePermission.AddRolePermissions(
                toAdd.Select(id => new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = id
                }).ToList()
            );

            await _repoRole.SaveChangesAsync();

            // role permissions thay đổi -> clear toàn bộ permission cache
            await _permissionCacheService
                .ClearAllPermissionsCacheAsync();

            return new RolePermissionResponseDTO
            {
                RoleId = roleId,
                ProcessedIds = toAdd,
                FailedIds = failedIds,
                Message = "Thêm quyền thành công"
            };
        }

        // ================= REMOVE PERMISSIONS =================
        public async Task<RolePermissionResponseDTO> RemovePermissionsAsync(
        int roleId,
        List<int> permissionIds)
        {
            ValidateInput(permissionIds);

            var role = await _repoRole.FindByIdAsync(roleId)
                ?? throw new NotFoundException("Không tìm thấy vai trò");

            ValidateRole(role);

            // ================= DATA SCOPE =================
            var currentRoles = _currentUserService.Roles
                .Select(r => Enum.Parse<RoleType>(r))
                .ToList();

            if (!_userDataScopeService.CanManageRole(
                    currentRoles,
                    role))
            {
                throw new ForbiddenException(
                    "Không có quyền chỉnh sửa vai trò ADMIN");
            }

            //Lấy các mapping tồn tại trong DB
            var entities =
                await _repoRolePermission.GetRolePermissionsAsync(
                    roleId,
                    permissionIds);

            //Nếu không có cái nào tồn tại
            if (!entities.Any())
            {
                return new RolePermissionResponseDTO
                {
                    RoleId = roleId,
                    ProcessedIds = new List<int>(),
                    FailedIds = permissionIds,
                    Message = "Không tìm thấy quyền nào trong vai trò này"
                };
            }

            //danh sách permission thực sự sẽ bị xóa
            var removedIds =
                entities.Select(x => x.PermissionId).ToList();

            //danh sách fail (không tồn tại trong role)
            var failedIds =
                permissionIds.Except(removedIds).ToList();

            //remove mapping
            _repoRolePermission.RemoveRolePermissions(entities);

            await _repoRole.SaveChangesAsync();

            // role permissions thay đổi -> clear toàn bộ permission cache
            await _permissionCacheService
                .ClearAllPermissionsCacheAsync();

            return new RolePermissionResponseDTO
            {
                RoleId = roleId,
                ProcessedIds = removedIds,
                FailedIds = failedIds,
                Message = "Xóa quyền thành công"
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
                throw new BadRequestException("Danh sách PermissionIds không được để trống");
        }
        // check quyền được phép sửa
        private static void ValidateRole(Role role)
        {
            if (role.Name == RoleType.ADMIN)
                throw new ForbiddenException("Không được phép chỉnh sửa vai trò ADMIN");
        }

        public class RoleDataScopeService
        {
            public bool CanManageRole(
                IEnumerable<RoleType> currentRoles,
                Role targetRole)
            {
                // Chỉ ADMIN được quản lý permission của role

                if (!currentRoles.Contains(RoleType.ADMIN))
                    return false;

                // Không cho sửa permission của chính role ADMIN

                if (targetRole.Name == RoleType.ADMIN)
                    return false;

                return true;
            }
        }
    }
}
