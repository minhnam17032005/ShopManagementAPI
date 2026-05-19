using Demo_Course_Management.Data;
using Demo_Course_Management.DTOs;
using Demo_Course_Management.DTOs.request;
using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.Jwt;
using Demo_Course_Management.Middleware;
using Demo_Course_Management.Models.Enum;
using Demo_Course_Management.Models;
using Demo_Course_Management.Repositories;
using Microsoft.EntityFrameworkCore;
using Azure;
using Azure.Core;

namespace Demo_Course_Management.Services
{
    public class AuthService
    {
        private readonly UserRepository _repoUser;
        private readonly RoleRepository _repoRole;
        private readonly JwtService _jwtService;
        private readonly CurrentUserService _currentUser;
        private readonly IConfiguration _config;
        private readonly JwtBlacklistService _jwtBlacklist;


        public AuthService(UserRepository repoUser, RoleRepository repoRole,
            JwtService jwtService, CurrentUserService currentUser, IConfiguration config, JwtBlacklistService jwtBlacklist)
        {
            _repoUser = repoUser;
            _repoRole = repoRole;
            _jwtService = jwtService;
            _currentUser = currentUser;
            _config = config;
            _jwtBlacklist = jwtBlacklist;
        }
        public async Task<(LoginResponseDTO response, string refreshToken)> LoginAsync(LoginRequestDTO dto)
        {
            //check login và so sánh password 
            var user = await _repoUser.GetByUsernameWithRolesAsync(dto.Username);
            bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password,user.PasswordHash);
            if (user == null||!isValid)
                throw new UnauthorizedException("Username hoặc password không hợp lệ");
            
            // check account lock 
            if (!user.IsActive)
                throw new UnauthorizedException("Tài khoản đã bị khóa.");

            //lấy ra access token
            var token = _jwtService.GenerateAccessToken(user);

            // tạo và gán refresh token
            var refreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiredAt = DateTime.UtcNow.AddDays(
                int.Parse(_config["Jwt:RefreshTokenExpirationDays"])
            );
            await _repoUser.SaveAsync();

            var response = new LoginResponseDTO
            {
                User = new LoginResponseDTO.UserInfo
                {
                Id = user.Id,
                Username = user.Username,
                Roles = user.UserRoles
                    .Select(x => new RoleItemDTO{
                        Id = x.RoleId,
                        Name = x.Role.Name
                        })
                        .ToList()},
                AccessToken = token
                };

            //trả ra LoginResponseDTO và refreshToken cho controller 
            return (response, refreshToken);
        }

        public async Task<( RefreshResponseDTO response , string refreshToken) >RefreshTokenAsync(string refreshToken)
        {
            // tìm user theo refresh token
            var user = await _repoUser.GetByRefreshTokenAsync(refreshToken);

            if (user == null)
                throw new UnauthorizedException("Phiên đăng nhập không hợp lệ.");

            if (!user.IsActive)
                throw new UnauthorizedException("Tài khoản đã bị khóa.");

            if (user.RefreshTokenExpiredAt < DateTime.UtcNow)
                throw new UnauthorizedException("Phiên đăng nhập đã hết hạn." );

            // tạo access token mới
            var accessToken = _jwtService.GenerateAccessToken(user);

            // rotate refresh token
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;

            user.RefreshTokenExpiredAt = DateTime.UtcNow.AddDays(
                int.Parse(_config["Jwt:RefreshTokenExpirationDays"])
            );

            await _repoUser.SaveAsync();

            //trả ra accesstoken và refreshtoken mới 
            return (new RefreshResponseDTO { AccessToken = accessToken }, newRefreshToken);
        }

        public async Task<RegisterResponseDTO> RegisterAsync(RegisterRequestDTO dto)
        {
            var existsUsername = await _repoUser.IsUsernameExists(dto.Username);
            if (existsUsername)
                throw new BadRequestException("Username already exists");

            var existsEmail = await _repoUser.IsEmailExists(dto.Email);
            if (existsEmail)
                throw new BadRequestException("Email already exists");

            //mặc định role khi đăng ký là CUSTOMER
            var customerRole = await _repoRole.GetByNameAsync(RoleType.CUSTOMER);
            if (customerRole == null)
                throw new Exception("Default role CUSTOMER not found");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Email = dto.Email,
                IsActive = true,
                UserRoles = new List<UserRole>
                {
                    new UserRole
                    {
                        RoleId = customerRole.Id
                    }
                }
            };
            _repoUser.AddAsync(user);
            await _repoUser.SaveAsync();

            return new RegisterResponseDTO
            {
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Message = "Register successfully"
            };
        }

        public async Task<UserResponseDTO> GetProfileAsync()
        {
            // lấy userId từ JWT
            var userId = _currentUser.UserId;

            // chưa đăng nhập / token invalid
            if (userId == 0){
                throw new UnauthorizedException("Bạn chưa đăng nhập hoặc phiên đăng nhập không hợp lệ.");
            }

            // query DB
            var user = await _repoUser.GetByIdWithRolesAsync(userId);

            // token cũ / user bị xóa / user bị khóa
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedException("Phiên đăng nhập không hợp lệ hoặc tài khoản đã bị khóa.");
            }
            return new UserResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive,
                Roles = user.UserRoles
                    .Select(x => new RoleItemDTO
                    {
                        Id = x.Role.Id,
                        Name = x.Role.Name
                    })
                    .ToList(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        public async Task LogoutAsync(string? jti,string? expClaim,string? refreshToken)
        {
            // blacklist access token
            if (!string.IsNullOrEmpty(jti) &&
                !string.IsNullOrEmpty(expClaim))
            {
                var expUnix = long.Parse(expClaim);

                var expiredAt = DateTimeOffset
                    .FromUnixTimeSeconds(expUnix)
                    .UtcDateTime;

                var ttl = expiredAt - DateTime.UtcNow;

                if (ttl > TimeSpan.Zero){
                    await _jwtBlacklist.BlacklistTokenAsync(jti, ttl);
                }
            }

            // remove refresh token
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var user = await _repoUser
                    .GetByRefreshTokenAsync(refreshToken);

                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiredAt = null;

                    await _repoUser.SaveAsync();
                }
            }
        }




    }
}
