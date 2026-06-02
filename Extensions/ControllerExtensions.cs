using Microsoft.AspNetCore.Mvc;
using ShopManagementAPI.DTOs.Common;

namespace ShopManagementAPI.Extensions
{
    public static class ControllerExtensions
    {
        public static ActionResult<ApiResponse<T>> ApiOk<T>(
            this ControllerBase controller,
            T data,
            string message = "Success")
        {
            return controller.Ok(
                new ApiResponse<T>
                {
                    Success = true,
                    StatusCode = StatusCodes.Status200OK,
                    Message = message,
                    Data = data
                });
        }

        public static ActionResult<ApiResponse<T>> ApiCreated<T>(
            this ControllerBase controller,
            T data,
            string message = "Created successfully")
        {
            return controller.StatusCode(
                StatusCodes.Status201Created,
                new ApiResponse<T>
                {
                    Success = true,
                    StatusCode = StatusCodes.Status201Created,
                    Message = message,
                    Data = data
                });
        }
    }
}