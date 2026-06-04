using System.Text.Json;
using System.Text.RegularExpressions;
using ShopManagementAPI.Authorization;
using ShopManagementAPI.Jwt;
using ShopManagementAPI.Repositories;
using ShopManagementAPI.DTOs.Common;

namespace ShopManagementAPI.Middleware
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;

        public PermissionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            CurrentUserService currentUser,
            UserRepository userRepository,
            PermissionCacheService permissionCacheService
        )
        {
            // lấy endpoint của request hiện tại
            var endpoint = context.GetEndpoint();
            
            // lấy attribute RequirePermission nếu có gắn trên API
            var permissionAttribute =
                endpoint?.Metadata
                    .GetMetadata<RequirePermissionAttribute>();

            // nếu API không yêu cầu quyền → bỏ qua check
            if (permissionAttribute == null)
            {
                await _next(context);
                return;
            }

            // lấy danh sách quyền của user từ cache
            var permissions =
                await permissionCacheService
                    .GetPermissionsAsync(currentUser.UserId);

            // kiểm tra permission của user 
            var hasPermission = permissions.Contains(
                    permissionAttribute.Permission
                );

            // nếu không có quyền → trả về 403
            if (!hasPermission)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode =StatusCodes.Status403Forbidden;

                var forbiddenResponse = new ErrorResponse
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Code = "FORBIDDEN",
                    Message = "Bạn không có quyền truy cập"
                };

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(
                        forbiddenResponse
                    )
                );

                return;
            }

            await _next(context);
        }
    }

}
