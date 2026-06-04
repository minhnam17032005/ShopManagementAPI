using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ShopManagementAPI.DTOs.Common;

namespace ShopManagementAPI.Jwt
{
    public class JwtAuthEvents : JwtBearerEvents
    {
        // TOKEN INVALID / EXPIRED
        public override async Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            context.NoResult();

            // đánh dấu đã xử lý lỗi authentication
            context.HttpContext.Items["AuthFailed"] = true;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            string code = "TOKEN_INVALID";
            string message = "Access token không hợp lệ";

            // TOKEN EXPIRED
            if (context.Exception is SecurityTokenExpiredException)
            {
                code = "TOKEN_EXPIRED";
                message = "Access token đã hết hạn";
            }

            // INVALID SIGNATURE
            else if (
                context.Exception
                is SecurityTokenInvalidSignatureException)
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

            var response = new ErrorResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Code = code,
                Message = message
            };

            await context.Response.WriteAsJsonAsync(response);
        }

        // NO TOKEN
        public override async Task Challenge(JwtBearerChallengeContext context)
        {
            // AuthenticationFailed đã xử lý rồi
            if (context.HttpContext.Items.ContainsKey("AuthFailed"))
            {
                return;
            }

            // response đã được ghi trước đó
            if (context.Response.HasStarted)
            {
                return;
            }

            context.HandleResponse();

            context.Response.ContentType = "application/json";
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            var response = new ErrorResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Code = "TOKEN_MISSING",
                Message = "Vui lòng đăng nhập"
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}