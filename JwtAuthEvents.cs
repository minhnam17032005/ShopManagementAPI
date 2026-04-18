namespace Demo_Course_Management
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.IdentityModel.Tokens;
    using System.Text.Json;

    public class JwtAuthEvents : JwtBearerEvents
    {
        public override Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            context.NoResult();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var message = context.Exception switch
            {
                SecurityTokenExpiredException =>
                    "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",

                SecurityTokenInvalidSignatureException =>
                    "Token không hợp lệ. Vui lòng đăng nhập lại.",

                _ =>
                    "Xác thực thất bại. Vui lòng kiểm tra lại token."
            };

            var result = JsonSerializer.Serialize(new
            {
                message
            });

            return context.Response.WriteAsync(result);
        }

        public override Task Challenge(JwtBearerChallengeContext context)
        {
            context.HandleResponse();

            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new
            {
                message = "Bạn chưa đăng nhập hoặc không có quyền truy cập."
            });

            return context.Response.WriteAsync(result);
        }
    }
}
