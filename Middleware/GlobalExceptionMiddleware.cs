namespace ShopManagementAPI.Middleware
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
            object response;

            switch (ex)
            {
                //khi thiếu dữ liệu 
                case BadRequestException badRequestEx:

                    statusCode = StatusCodes.Status400BadRequest;

                    response = new
                    {
                        status = statusCode,
                        errors = badRequestEx.Errors ?? new List<string>
                {
                    badRequestEx.Message
                }
                    };
                    break;
                    //không tìm thấy dữ liệu được nêu 
                case NotFoundException:

                    statusCode = StatusCodes.Status404NotFound;

                    response = new
                    {
                        status = statusCode,
                        message = ex.Message
                    };
                    break;
                    //khi trùng ,xung đột dữ liệu add hay tìm kiếm 
                case ConflictException:

                    statusCode = StatusCodes.Status409Conflict;

                    response = new
                    {
                        status = statusCode,
                        message = ex.Message
                    };
                    break;
                case UnauthorizedException://xác thực 
                    statusCode = StatusCodes.Status401Unauthorized;

                    response = new
                    {
                        status = statusCode,
                        message = ex.Message
                    };
                    break;
                case ForbiddenException://phân quyền 
                    statusCode = StatusCodes.Status403Forbidden;

                    response = new
                    {
                        status = statusCode,
                        message = ex.Message
                    };
                    break;

                //tạm thời 500 để lấy lỗi chính xác 
                default:
                    statusCode = StatusCodes.Status500InternalServerError;

                    response = new
                    {
                        status = statusCode,
                        message = ex.Message
                    };

                    break;
            }

            context.Response.StatusCode = statusCode;

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}
