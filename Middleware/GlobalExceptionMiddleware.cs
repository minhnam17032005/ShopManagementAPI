namespace Demo_Course_Management.Middleware
{
    using System.Net;
    using System.Text.Json;

    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            int statusCode;
            string message = ex.Message;

            switch (ex)
            {
                case BadRequestException:
                    statusCode = (int)HttpStatusCode.BadRequest; // 400
                    break;

                case NotFoundException:
                    statusCode = (int)HttpStatusCode.NotFound; // 404
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError; // 500
                    message = "Internal server error"; // ẩn lỗi thật
                    break;
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                status = statusCode,
                message = message,
                timestamp = DateTime.UtcNow,
                traceId = context.TraceIdentifier
            };

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}
