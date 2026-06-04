using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using ShopManagementAPI.DTOs.Common;
using ShopManagementAPI.Jwt;
using ShopManagementAPI.Repositories;

namespace ShopManagementAPI.Middleware
{
    public class JwtSecurityMiddleware
    {
        // đi tiếp middleware tiếp theo trong pipeline
        private readonly RequestDelegate _next;

        public JwtSecurityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            JwtBlacklistService jwtBlacklistService,
            UserRepository userRepository,
            CurrentUserService currentUser
        )
        {

            // chỉ xử lý khi user đã đăng nhập thành công
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // =========================
                // kiểm tra token bị thu hồi (blacklist)
                // =========================

                // lấy jti từ access token
                var jti = currentUser.Jti;

                // nếu có jti thì kiểm tra blacklist
                if (!string.IsNullOrEmpty(jti))
                {
                    var isBlacklisted =
                        await jwtBlacklistService.IsBlacklistedAsync(jti);

                    // token đã bị revoke
                    if (isBlacklisted)
                    {
                        context.Response.ContentType = "application/json";

                        context.Response.StatusCode =
                            StatusCodes.Status401Unauthorized;

                        var response = new ErrorResponse
                        {
                            StatusCode = StatusCodes.Status401Unauthorized,
                            Code = "TOKEN_REVOKED",
                            Message = "Access token đã bị thu hồi"
                        };

                        await context.Response.WriteAsync(
                            JsonSerializer.Serialize(response)
                        );

                        return;
                    }
                }

                // =========================
                // CHECK USER DISABLED
                // =========================
                var userIdClaim = currentUser.UserId;

                if (userIdClaim>0)
                {
                    var user =
                        await userRepository.GetByIdAsync(userIdClaim);

                    // user đã bị khóa
                    if (user != null && !user.IsActive)
                    {
                        context.Response.ContentType = "application/json";

                        context.Response.StatusCode =
                            StatusCodes.Status401Unauthorized;

                        var response = new ErrorResponse
                        {
                            StatusCode = StatusCodes.Status401Unauthorized,
                            Code = "USER_DISABLED",
                            Message = "Tài khoản đã bị khóa"
                        };

                        await context.Response.WriteAsync(
                            JsonSerializer.Serialize(response)
                        );

                        return;
                    }
                }
            }
            // token và user hợp lệ -> đi tiếp
            await _next(context);
        }
    }
}