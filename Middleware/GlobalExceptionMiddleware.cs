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
            object response;

            switch (ex)
            {
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

                case NotFoundException:

                    statusCode = StatusCodes.Status404NotFound;

                    response = new
                    {
                        status = statusCode,
                        message = ex.Message
                    };
                    break;
                case ConflictException:

                    statusCode = StatusCodes.Status409Conflict;

                    response = new
                    {
                        status = statusCode,
                        message = ex.Message
                    };
                    break;

                default:

                    statusCode = StatusCodes.Status500InternalServerError;

                    response = new
                    {
                        status = statusCode,
                        message = "Lỗi hệ thống."
                    };
                    break;
            }

            context.Response.StatusCode = statusCode;

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}
