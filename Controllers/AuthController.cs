using System.IdentityModel.Tokens.Jwt;
using Demo_Course_Management.DTOs.request;
using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.Middleware;
using Demo_Course_Management.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo_Course_Management.Controllers
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

            [HttpPost("register")]
            public async Task<ActionResult<RegisterResponseDTO>> Register(RegisterRequestDTO dto)
            {
                var result = await _authService.RegisterAsync(dto);

                return StatusCode(201, result);
            }

            [Authorize]
            [HttpGet("profile")]
            public async Task<ActionResult<UserResponseDTO>> GetProfile()
            {
                var result = await _authService.GetProfileAsync();
                return Ok(result);
            }

            [Authorize]
            [HttpPost("logout")]
            public async Task<IActionResult> Logout()
            {
                var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
                
                // lấy refresh token từ cookie
                var refreshToken = Request.Cookies["refreshToken"];

                // gọi service logout
                await _authService.LogoutAsync(jti,expClaim,refreshToken);

                // xóa refresh token trên cookie
                Response.Cookies.Delete("refreshToken");

                return Ok(new{
                    message = "Đăng xuất thành công."
                });
            }
    }
}
