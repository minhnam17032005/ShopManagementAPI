using System.Data;
using System.Data.Common;
using Demo_Course_Management.DTOs.request;
using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.Middleware;
using Demo_Course_Management.Models;
using Demo_Course_Management.Models.Enum;
using Demo_Course_Management.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Demo_Course_Management.Services
{
    public class UserService
    {
        private readonly UserRepository _repoUser;
        private readonly RoleRepository _repoRole;
        private readonly OrderRepository _repoOrder;

        public UserService(UserRepository repoUser, RoleRepository repoRole, OrderRepository repoOrder)
        {
            _repoUser = repoUser;
            _repoRole = repoRole;
            _repoOrder = repoOrder;
        }

        public async Task<UserResponseDTO> CreateAsync(CreateUserReqDTO dto)
        {
            // check username
            if (await _repoUser.IsUsernameExists(dto.Username))
                throw new BadRequestException("Username already exists");

            // check email
            if (await _repoUser.IsEmailExists(dto.Email))
                throw new BadRequestException("Email already exists");

            // check role
            var role = await _repoRole.GetRoleById(dto.RoleId);
            if (role == null)
                throw new NotFoundException("Role not found");
            // CHẶN tạo ADMIN
            if (role.Name == RoleType.ADMIN)
                throw new BadRequestException("Cannot create ADMIN");

            // hash password
            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = hash,
                FullName = dto.FullName,
                Email = dto.Email,
                RoleId = role.Id,
                IsActive = true
            };

            await _repoUser.AddAsync(user);
            await _repoUser.SaveAsync();

            // map response
            return MapToDTO(user,role);
        }

        public async Task<UserResponseDTO> UpdateAsync(int id, UpdateUserReqDTO dto)
        {
            // check user
            var user = await _repoUser.GetByIdAsync(id);
            if (user == null)
                throw new NotFoundException("User not found");
            if (!user.IsActive)
                throw new BadRequestException("Tài khoản đã bị khóa.");

            // check role
            var role = await _repoRole.GetRoleById(dto.RoleId);
            if (role == null)
                throw new NotFoundException("Role not found");

            // chặn update ADMIN 
            if (role.Name == RoleType.ADMIN)
                throw new BadRequestException("Cannot assign ADMIN");

            // update
            user.FullName = dto.FullName;
            user.RoleId = dto.RoleId;
            user.UpdatedAt = DateTime.Now;

            await _repoUser.SaveAsync();

            return MapToDTO(user, role);
        }

        public async Task<List<UserResponseDTO>> GetAllAsync()
        {
            var users = await _repoUser.GetAllAsync();

            //sau có phân quyền sẽ phần quyền lại xem ai được xem gì 
            return users.Select(user => MapToDTO(user)).ToList();
        }

        public async Task<UserResponseDTO> GetByIdAsync(int id)
        {
            var user = await _repoUser.GetByIdWithRoleAsync(id);

            if (user == null)
                throw new NotFoundException("User not found");
            
            //sau có phân quyền sẽ phần quyền lại xem ai được xem gì 
            return MapToDTO(user);
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _repoUser.GetByIdAsync(id);

            if (user == null)
                throw new NotFoundException("User not found");
            if (!user.IsActive)
                throw new BadRequestException("Tài khoản đã bị khóa.");

            // Không cho khóa Admin
            if (user.Role?.Name == RoleType.ADMIN)
                    throw new BadRequestException("Không được khóa tài khoản Admin.");

            // Chỉ chặn nếu còn đơn Pending
            var hasPendingOrders = await _repoOrder.AnyPendingByUserIdAsync(id);

            if (hasPendingOrders)
                throw new ConflictException(
                    "Người dùng đang có đơn hàng chờ xử lý nên không thể khóa.");

            // Các trạng thái khác (Completed / Cancelled) -> cho khóa
            user.IsActive = false;
            user.UpdatedAt = DateTime.Now;

            await _repoUser.SaveAsync();
        }

        public async Task RestoreAsync(int id)
        {
            var user = await _repoUser.GetByIdAsync(id);

            if (user == null)
                throw new NotFoundException("User not found");

            if (user.IsActive)
                throw new BadRequestException("Tài khoản đang hoạt động.");

            user.IsActive = true;
            user.UpdatedAt = DateTime.Now;

            await _repoUser.SaveAsync();
        }
        public static UserResponseDTO MapToDTO(User user, Role role)
        {
            return new UserResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive,
                RoleId = user.RoleId,
                RoleName = role.Name,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
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
                RoleId = user.RoleId,
                RoleName = (RoleType)(user.Role?.Name),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }


    }
}
