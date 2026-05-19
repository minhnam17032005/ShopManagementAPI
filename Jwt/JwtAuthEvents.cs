using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace Demo_Course_Management.Jwt
{
    public class JwtAuthEvents : JwtBearerEvents
    {
        private readonly JwtBlacklistService _blacklistService;

        public JwtAuthEvents(JwtBlacklistService blacklistService)
        {
            _blacklistService = blacklistService;
        }

        // 1. TOKEN VALIDATED (BLACKLIST CHECK)
        public override async Task TokenValidated(TokenValidatedContext context)
        {
            var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (!string.IsNullOrEmpty(jti))
            {
                var isBlacklisted = await _blacklistService.IsBlacklistedAsync(jti);

                if (isBlacklisted)
                {
                    context.Fail("Token đã bị thu hồi (logout).");
                    return;
                }
            }

            await base.TokenValidated(context);
        }

        // 2. AUTH FAILED (TOKEN INVALID / EXPIRED)
        public override Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            context.NoResult();

            if (context.Response.HasStarted)
                return Task.CompletedTask;

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var message = context.Exception switch
            {
                SecurityTokenExpiredException =>
                    "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",

                SecurityTokenInvalidSignatureException =>
                    "Token không hợp lệ. Vui lòng đăng nhập lại.",

                _ =>
                    "Xác thực thất bại. Vui lòng đăng nhập lại."
            };

            var result = JsonSerializer.Serialize(new
            {
                success = false,
                statusCode = 401,
                message
            });

            return context.Response.WriteAsync(result);
        }

        // 3. CHALLENGE (NO TOKEN / 401 DEFAULT)
        public override Task Challenge(JwtBearerChallengeContext context)
        {
            context.HandleResponse();
            

            if (context.Response.HasStarted)
                return Task.CompletedTask;

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new
            {
                success = false,
                statusCode = 401,
                message = "Bạn chưa đăng nhập hoặc token không hợp lệ."
            });

            return context.Response.WriteAsync(result);
        }
    }
}