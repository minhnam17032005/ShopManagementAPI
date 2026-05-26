using System.IdentityModel.Tokens.Jwt;
using ShopManagementAPI.DTOs.request;
using ShopManagementAPI.DTOs.response;
using ShopManagementAPI.Middleware;
using ShopManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ShopManagementAPI.Jwt;

namespace ShopManagementAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IConfiguration _config;
        private readonly CurrentUserService _currentUser;

        public AuthController(AuthService authService, IConfiguration config, CurrentUserService currentUser)
        {
            _authService = authService;
            _config = config;
            _currentUser = currentUser;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login(LoginRequestDTO dto)
        {
            var (response, refreshToken) = await _authService.LoginAsync(dto);

            // set cookie
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true, // không cho JS đọc (chống XSS)

                Secure = true, // chỉ gửi qua HTTPS (prod bắt buộc)

                SameSite = SameSiteMode.Strict, // chỉ gửi cùng domain (chống CSRF)

                Expires = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpirationDays"])
                )
            });
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<RefreshResponseDTO>> Refresh()
        {
            // lấy refresh token từ cookie
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedException("Unauthorized");

            // gọi service
            var (response, newRefreshToken) = await _authService.RefreshTokenAsync(refreshToken);

            // set cookie mới
            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(
                    int.Parse(_config["Jwt:RefreshTokenExpirationDays"])
                )
            });

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDTO>> Register(RegisterRequestDTO dto)
        {
            var result = await _authService.RegisterAsync(dto);

            return StatusCode(201, result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserResponseDTO>> GetProfile()
        {
            var result = await _authService.GetProfileAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<SuccessResponseDTO>> Logout()
        {
            //lấy Jti và ExpiredAt từ CurrentUserService
            var jti = _currentUser.Jti;
            var expClaim = _currentUser.ExpiredAtString;

            // lấy refresh token từ cookie
            var refreshToken = Request.Cookies["refreshToken"];

            // gọi service logout
            await _authService.LogoutAsync(jti, expClaim, refreshToken);

            // xóa refresh token trên cookie
            Response.Cookies.Delete("refreshToken");

            return Ok(new
            {
                success = true,
                message = "Đăng xuất thành công."
            });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<SuccessResponseDTO>> ChangePassword(ChangePasswordRequestDTO request)
        {
            // lấy user id từ token
            var userId = _currentUser.UserId;
            // lấy jti access token
            var jti = _currentUser.Jti;
            
            await _authService.ChangePasswordAsync(
                userId,
                request,
                jti
            );
            return Ok(new
            {
                success = true,
                message = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại."
            });
        }
    }
}
