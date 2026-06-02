using ShopManagementAPI.DTOs.request;
using ShopManagementAPI.DTOs.response;
using ShopManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementAPI.Authorization;
using ShopManagementAPI.DTOs.Common;
using ShopManagementAPI.Extensions;

namespace ShopManagementAPI.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    [Produces("application/json")]
    public class PermissionController : ControllerBase
    {
        private readonly PermissionService _service;

        public PermissionController(PermissionService service)
        {
            _service = service;
        }

        [Authorize]
        [RequirePermission(Permissions.GetPermissions)]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<PermissionResponseDTO>>>> GetAll()
        {
            var result = await _service.GetAllAsync();

            return this.ApiOk(
                result,
                "Lấy danh sách permission thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.GetPermissionDetail)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PermissionResponseDTO>>> GetById(
            int id)
        {
            var result = await _service.GetByIdAsync(id);

            return this.ApiOk(
                result,
                "Lấy chi tiết permission thành công"
            );
        }


    }
}
