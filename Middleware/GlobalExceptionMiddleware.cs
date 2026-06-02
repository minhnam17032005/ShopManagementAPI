using System.Text.Json;
using ShopManagementAPI.DTOs.Common;
using ShopManagementAPI.Exceptions;

namespace ShopManagementAPI.Middleware
{
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

        private static async Task HandleExceptionAsync(
            HttpContext context,
            Exception ex)
        {
            context.Response.ContentType = "application/json";

            int statusCode;
            ErrorResponse response;

            switch (ex)
            {
                // Validation
                case BadRequestException badRequestEx:

                    statusCode = StatusCodes.Status400BadRequest;

                    response = new ErrorResponse
                    {
                        StatusCode = statusCode,
                        Code = "BAD_REQUEST",
                        Message = badRequestEx.Message,
                        Errors = badRequestEx.Errors
                    };
                    break;

                // Not Found
                case NotFoundException:

                    statusCode = StatusCodes.Status404NotFound;

                    response = new ErrorResponse
                    {
                        StatusCode = statusCode,
                        Code = "NOT_FOUND",
                        Message = ex.Message
                    };
                    break;

                // Conflict
                case ConflictException:

                    statusCode = StatusCodes.Status409Conflict;

                    response = new ErrorResponse
                    {
                        StatusCode = statusCode,
                        Code = "CONFLICT",
                        Message = ex.Message
                    };
                    break;

                // Unauthorized
                case UnauthorizedException:

                    statusCode = StatusCodes.Status401Unauthorized;

                    response = new ErrorResponse
                    {
                        StatusCode = statusCode,
                        Code = "UNAUTHORIZED",
                        Message = ex.Message
                    };
                    break;

                // Forbidden
                case ForbiddenException:

                    statusCode = StatusCodes.Status403Forbidden;

                    response = new ErrorResponse
                    {
                        StatusCode = statusCode,
                        Code = "FORBIDDEN",
                        Message = ex.Message
                    };
                    break;

                // Internal Server Error
                default:

                    statusCode = StatusCodes.Status500InternalServerError;

                    response = new ErrorResponse
                    {
                        StatusCode = statusCode,
                        Code = "INTERNAL_SERVER_ERROR",
                        Message = ex.Message
                    };
                    break;
            }

            context.Response.StatusCode = statusCode;

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}