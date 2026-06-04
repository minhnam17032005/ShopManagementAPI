using System.IdentityModel.Tokens.Jwt;
using ShopManagementAPI.DTOs.request;
using ShopManagementAPI.DTOs.response;

using ShopManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ShopManagementAPI.Jwt;
using ShopManagementAPI.Exceptions;
using ShopManagementAPI.DTOs.Common;
using ShopManagementAPI.Extensions;

namespace ShopManagementAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IConfiguration _config;

        public AuthController(AuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login(LoginRequestDTO dto)
        {
            var (response, refreshToken) =
                await _authService.LoginAsync(dto);

            // set cookie
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true, // không cho JS đọc (chống XSS)

                Secure = true, // chỉ gửi qua HTTPS (prod bắt buộc)

                SameSite = SameSiteMode.Strict, // chỉ gửi cùng domain (chống CSRF)

                Expires = DateTime.UtcNow.AddDays(
                    int.Parse(_config["Jwt:RefreshTokenExpirationDays"])
                )
            });

            return this.ApiOk(
                response,
                "Đăng nhập thành công"
            );
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<RefreshResponseDTO>>> Refresh()
        {
            // lấy refresh token từ cookie
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedException("Refresh token không tồn tại hoặc đã bị xóa");

            // gọi service
            var (response, newRefreshToken) =
                await _authService.RefreshTokenAsync(refreshToken);

            // set refresh token mới vào cookie 
            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(
                    int.Parse(_config["Jwt:RefreshTokenExpirationDays"])
                )
            });

            return this.ApiOk(
                response,
                "Làm mới token thành công"
            );
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<RegisterResponseDTO>>> Register(
            RegisterRequestDTO dto)
        {
            var result = await _authService.RegisterAsync(dto);

            return this.ApiCreated(
                result,
                "Đăng ký tài khoản thành công"
            );
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserProfileResponseDTO>>> GetProfile()
        {
            var result = await _authService.GetProfileAsync();

            return this.ApiOk(
                result,
                "Lấy thông tin cá nhân thành công"
            );
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<object>>> Logout()
        {
            // lấy refresh token từ cookie
            var refreshToken = Request.Cookies["refreshToken"];

            // gọi service logout
            await _authService.LogoutAsync(refreshToken);

            // xóa refresh token trên cookie
            Response.Cookies.Delete("refreshToken");

            return this.ApiOk<object>(
                null,
                "Đăng xuất thành công"
            );
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
            ChangePasswordRequestDTO request)
        {
            await _authService.ChangePasswordAsync(request);

            return this.ApiOk<object>(
                null,
                "Đổi mật khẩu thành công. Vui lòng đăng nhập lại."
            );
        }
    }
}
