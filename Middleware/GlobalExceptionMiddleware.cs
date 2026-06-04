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

        private static async Task HandleExceptionAsync(HttpContext context,Exception ex)
        {
            // response đã được ghi trước đó
            if (context.Response.HasStarted)
            {
                return;
            }
            context.Response.Clear();
            context.Response.ContentType = "application/json";

            int statusCode;
            ErrorResponse response;

            switch (ex)
            {
                // 400 : lỗi validate dữ liệu đầu vào
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

                // 404 : không tìm thấy dữ liệu
                case NotFoundException:

                    statusCode = StatusCodes.Status404NotFound;

                    response = new ErrorResponse
                    {
                        StatusCode = statusCode,
                        Code = "NOT_FOUND",
                        Message = ex.Message
                    };
                    break;

                // 409 : xung đột dữ liệu
                case ConflictException:

                    statusCode = StatusCodes.Status409Conflict;

                    response = new ErrorResponse
                    {
                        StatusCode = statusCode,
                        Code = "CONFLICT",
                        Message = ex.Message
                    };
                    break;

                // 401 : chưa đăng nhập / sai token
                case UnauthorizedException:

                    statusCode = StatusCodes.Status401Unauthorized;

                    response = new ErrorResponse
                    {
                        StatusCode = statusCode,
                        Code = "UNAUTHORIZED",
                        Message = ex.Message
                    };
                    break;

                // 403 : không có quyền truy cập
                case ForbiddenException:

                    statusCode = StatusCodes.Status403Forbidden;

                    response = new ErrorResponse
                    {
                        StatusCode = statusCode,
                        Code = "FORBIDDEN",
                        Message = ex.Message
                    };
                    break;

                // 500 : lỗi hệ thống
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

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}