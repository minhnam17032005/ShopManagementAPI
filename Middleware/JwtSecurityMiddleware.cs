using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
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

            // chỉ check nếu đã authenticate
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // =========================
                // CHECK TOKEN REVOKED
                // =========================
                // lấy jti từ access token
                var jti = currentUser.Jti;

                // nếu có jti thì check blacklist
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

                        var response = new
                        {
                            success = false,
                            statusCode = StatusCodes.Status401Unauthorized,
                            code = "TOKEN_REVOKED",
                            message = "Access token đã bị thu hồi",
                            timestamp = DateTime.UtcNow
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

                    // user bị khóa
                    if (user != null && !user.IsActive)
                    {
                        context.Response.ContentType = "application/json";

                        context.Response.StatusCode =
                            StatusCodes.Status401Unauthorized;

                        var response = new
                        {
                            success = false,
                            statusCode = StatusCodes.Status401Unauthorized,
                            code = "USER_DISABLED",
                            message = "Tài khoản đã bị khóa",
                            timestamp = DateTime.UtcNow
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