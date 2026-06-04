using System.Data;
using System.Data.Common;
using ShopManagementAPI.DTOs;
using ShopManagementAPI.DTOs.request;
using ShopManagementAPI.DTOs.response;
using ShopManagementAPI.Exceptions;
using ShopManagementAPI.Models;
using ShopManagementAPI.Models.Enum;
using ShopManagementAPI.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using ShopManagementAPI.Authorization;
using ShopManagementAPI.Jwt;
using Microsoft.EntityFrameworkCore;

namespace ShopManagementAPI.Services
{
    public class UserService
    {
        private readonly UserRepository _repoUser;
        private readonly UserRoleRepository _repoUserRole;
        private readonly RoleRepository _repoRole;
        private readonly OrderRepository _repoOrder;
        private readonly PermissionCacheService _permissionCacheService;
        private readonly CurrentUserService _currentUserService;
        private readonly UserDataScopeService _userDataScopeService;
      

        public UserService(UserRepository repoUser, RoleRepository repoRole, CurrentUserService currentUserService,
            UserRoleRepository repoUserRole, OrderRepository repoOrder, PermissionCacheService permissionCacheService, UserDataScopeService userDataScopeService)
        {
            _repoUser = repoUser;
            _repoRole = repoRole;
            _repoUserRole = repoUserRole;
            _repoOrder = repoOrder;
            _permissionCacheService = permissionCacheService;
            _currentUserService = currentUserService;
            _userDataScopeService = userDataScopeService;
        }

        public async Task<UserResponseDTO> CreateAsync(CreateUserReqDTO dto)
        {
            // kiểm tra username đã tồn tại
            if (await _repoUser.IsUsernameExists(dto.Username))
                throw new ConflictException("Tên đăng nhập đã tồn tại");

            // kiểm tra email đã tồn tại
            if (await _repoUser.IsEmailExists(dto.Email))
                throw new ConflictException("Email đã tồn tại");

            // kiểm tra danh sách role
            if (dto.RoleIds == null || !dto.RoleIds.Any())
                throw new BadRequestException("Danh sách RoleIds không được để trống");

            // lấy roles từ DB
            var errors = new List<string>();

            // xử lý role trùng lặp
            var roleIds = dto.RoleIds.Distinct().ToList();

            var roles = await _repoRole.GetRolesByIdsAsync(roleIds);
            var foundIds = roles.Select(x => x.Id).ToHashSet();

            //custom errors list
            foreach (var roleId in roleIds)
            {
                var role = roles.FirstOrDefault(x => x.Id == roleId);

                // không tìm thấy role
                if (role == null)
                {
                    errors.Add($"Không tìm thấy RoleId {roleId}");
                    continue;
                }
                // không cho phép gán ADMIN
                if (role.Name == RoleType.ADMIN)
                {
                    errors.Add($"RoleId {roleId} là ADMIN và không thể gán");
                }
            }
            if (errors.Any())
                throw new BadRequestException(errors);

            // hash password
            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = hash,
                FullName = dto.FullName,
                Email = dto.Email,
                IsActive = true,

                UserRoles = roles.Select(role => new UserRole
                {
                    RoleId = role.Id
                }).ToList()
            };
            await _repoUser.AddAsync(user);
            await _repoUser.SaveAsync();

            return MapToDTO(user);
        }

        public async Task<UserResponseDTO> ChangeProfileAsync(ChangeProfileReqDTO dto)
        {
            var userId = _currentUserService.UserId;

            var user = await _repoUser.GetByIdWithRolesAsync(userId);

            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng");

            // cập nhật profile
            user.FullName = dto.FullName;
            user.UpdatedAt = DateTime.UtcNow;

            await _repoUser.SaveAsync();

            return MapToDTO(user);
        }

        public async Task<UserResponseDTO> AddRolesAsync(int userId, List<int> roleIds)
        {
            // tìm user
            var user = await _repoUser.GetByIdWithRolesAsync(userId)
                ?? throw new NotFoundException("Không tìm thấy người dùng.");

            // validate roleIds
            var newRoleIds = roleIds?.Distinct().ToList()
                ?? throw new BadRequestException("Danh sách vai trò không được để trống.");

            var roles = await _repoRole.GetRolesByIdsAsync(newRoleIds);
            var roleDict = roles.ToDictionary(x => x.Id);

            // build errors list
            var errors = new List<string>();

            // role không tồn tại
            errors.AddRange(newRoleIds
                .Where(id => !roleDict.ContainsKey(id))
                .Select(id => $"Vai trò ID {id} không tồn tại."));
            // không cho gán ADMIN
            errors.AddRange(roleDict.Values
                .Where(r => r.Name == RoleType.ADMIN)
                .Select(r => "Không được phép gán quyền ADMIN cho người dùng."));

            if (errors.Any())
                throw new BadRequestException(errors);

            // lấy role chưa có
            var currentIds = user.UserRoles.Select(x => x.RoleId).ToList();
            var addIds = newRoleIds.Except(currentIds).ToList();

            if (!addIds.Any())
                throw new ConflictException("Người dùng đã có các vai trò này.");

            //thêm những roles mới 
            user.UserRoles.AddRange(addIds.Select(id => new UserRole
            {
                UserId = user.Id,
                RoleId = id
            }));

            await _repoUser.SaveAsync();
            // role user thay đổi -> clear permission cache
            await _permissionCacheService.RemovePermissionsAsync(userId);

            return MapToDTO(user);
        }

        public async Task<UserResponseDTO> RemoveRolesAsync(int userId, List<int> roleIds)
        {
            var user = await _repoUser.GetByIdWithRolesAsync(userId)
                ?? throw new NotFoundException("Không tìm thấy người dùng.");

            var removeIds = roleIds?.Distinct().ToList()
                ?? throw new BadRequestException("Danh sách vai trò không được để trống.");

            // check tồn tại lấy roles cần xóa
            var existingRoles = user.UserRoles
                .Where(x => removeIds.Contains(x.RoleId))
                .ToList();

            if (!existingRoles.Any())
                throw new NotFoundException("Không tìm thấy vai trò nào tồn tại");

            // không cho xóa hết role
            var remainingCount = user.UserRoles.Count - existingRoles.Count;
            if (remainingCount <= 0)
                throw new BadRequestException("Người dùng phải có ít nhất một vai trò.");

            _repoUserRole.RemoveRange(existingRoles);

            await _repoUser.SaveAsync();
            // role user thay đổi -> clear permission cache
            await _permissionCacheService
                .RemovePermissionsAsync(userId);

            return MapToDTO(user);
        }


        public async Task<List<UserResponseDTO>> GetAllAsync()
        {
            // lấy role hiện tại của user
            var currentUserRoles = _currentUserService.Roles
                .Select(r => Enum.Parse<RoleType>(r))
                .ToList();

            var query = _repoUser.Query();

            // filter theo data scope
            query = _userDataScopeService
                .FilterUsers(query, currentUserRoles);

            var users = await query.ToListAsync();

            return users
                .Select(MapToDTO)
                .ToList();
        }

        public async Task<UserResponseDTO> GetByIdAsync(int id)
        {   
            // lấy role hiện tại
            var user = await _repoUser.GetByIdWithRolesAsync(id);

            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng");

            // lấy danh sách role của user hiện tại
            var currentRoles = _currentUserService.Roles
                .Select(r => Enum.Parse<RoleType>(r))
                .ToList();

            // kiểm tra quyền xem user
            if (!_userDataScopeService.CanViewUser(currentRoles, user))
            {
                throw new ForbiddenException("Bạn không có quyền xem người dùng này");
            }

            return MapToDTO(user);
        }

        public async Task<StatusResponseDTO> LockAsync(int id)
        {
            // kiểm tra quyền lock user
            var user = await _repoUser.GetByIdWithRolesAsync(id);

            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng");

            var currentRoles = _currentUserService.Roles
                .Select(r => Enum.Parse<RoleType>(r))
                .ToList();

            // kiểm tra quyền khóa user
            if (!_userDataScopeService.CanManageUser(currentRoles, user))
            {
                throw new ForbiddenException("Bạn không có quyền khóa người dùng này");
            }

            if (!user.IsActive)
                throw new BadRequestException("Tài khoản đã bị khóa.");

            // không cho tự khóa chính mình
            if (user.Id == _currentUserService.UserId)
                throw new BadRequestException("Không thể tự khóa tài khoản của chính mình.");

            // kiểm tra đơn hàng đang xử lý
            var hasPendingOrders =
                await _repoOrder.AnyPendingByUserIdAsync(id);

            if (hasPendingOrders)
                throw new ConflictException("Người dùng đang có đơn hàng chờ xử lý nên không thể khóa.");

            // khóa user
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _repoUser.SaveAsync();

            await _permissionCacheService
                .RemovePermissionsAsync(id);

            return new StatusResponseDTO
            {
                IsActive = false
            };
        }

        public async Task<StatusResponseDTO> UnlockAsync(int id)
        {
            var user = await _repoUser.GetByIdWithRolesAsync(id);

            // kiểm tra user tồn tại
            if (user == null)
                throw new NotFoundException("Không tìm thấy người dùng");

            // lấy role của user hiện tại
            var currentRoles = _currentUserService.Roles
                .Select(r => Enum.Parse<RoleType>(r))
                .ToList();

            // kiểm tra quyền mở khóa user
            if (!_userDataScopeService.CanManageUser(currentRoles, user))
            {
                throw new ForbiddenException("Bạn không có quyền mở khóa người dùng này");
            }

            if (user.IsActive)
                throw new BadRequestException("Tài khoản đang hoạt động.");

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _repoUser.SaveAsync();

            await _permissionCacheService
                .RemovePermissionsAsync(id);

            return new StatusResponseDTO
            {
                IsActive = true
            };
        }
        public static UserResponseDTO MapToDTO(User user)
        {
            return new UserResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive,
                Roles = user.UserRoles
                    .Select(x => new RoleItemDTO{
                        Id = x.Role.Id,
                        Name = x.Role.Name
                    })
                    .ToList(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

    }
}
