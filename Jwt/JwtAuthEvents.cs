using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ShopManagementAPI.Jwt
{
    public class JwtAuthEvents : JwtBearerEvents
    {
        // TOKEN INVALID / EXPIRED
        public override async Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            context.NoResult();

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            string code = "TOKEN_INVALID";
            string message = "Access token không hợp lệ";

            // TOKEN EXPIRED
            if (context.Exception is SecurityTokenExpiredException)
            {
                code = "TOKEN_EXPIRED";
                message = "Access token đã hết hạn";
            }

            // INVALID SIGNATURE
            else if (context.Exception is SecurityTokenInvalidSignatureException)
            {
                code = "INVALID_SIGNATURE";
                message = "Chữ ký token không hợp lệ";
            }

            // MALFORMED TOKEN
            else if (
                context.Exception is SecurityTokenMalformedException ||
                context.Exception is ArgumentException
            )
            {
                code = "MALFORMED_TOKEN";
                message = "Token không đúng định dạng";
            }

            var response = new
            {
                success = false,
                statusCode = StatusCodes.Status401Unauthorized,
                code,
                message,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response)
            );
        }

        // NO TOKEN
        public override async Task Challenge(JwtBearerChallengeContext context)
        {
            // tránh ghi đè response nếu AuthenticationFailed đã xử lý
            if (context.Response.HasStarted)
            {
                return;
            }

            context.HandleResponse();

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            var response = new
            {
                success = false,
                statusCode = StatusCodes.Status401Unauthorized,
                code = "TOKEN_MISSING",
                message = "Vui lòng đăng nhập",
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response)
            );
        }
    }
}