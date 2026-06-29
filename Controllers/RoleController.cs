using ShopManagementAPI.DTOs.response;
using ShopManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementAPI.Authorization;
using ShopManagementAPI.DTOs.Common;
using ShopManagementAPI.Extensions;
using ShopManagementAPI.DTOs.request.Role;
using ShopManagementAPI.DTOs.response.Role;

namespace ShopManagementAPI.Controllers
{
    [ApiController]
    [Route("api/roles")]
    [Produces("application/json")]
    public class RoleController : ControllerBase
    {
        private readonly RoleService _service;

        public RoleController(RoleService service)
        {
                _service = service;
        }

        [Authorize]
        [RequirePermission(Permissions.GetRoles)]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<RoleResponseDTO>>>> GetAll()
        {
            var result = await _service.GetAllAsync();

            return this.ApiOk(
                result,
                "Lấy danh sách role thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.GetRoleDetail)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RoleResponseDTO>>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            return this.ApiOk(
                result,
                "Lấy chi tiết role thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.AddRolePermissions)]
        [HttpPost("{id}/permissions")]
        public async Task<ActionResult<ApiResponse<RolePermissionResponseDTO>>> AddPermissions(
            int id,
            [FromBody] RolePermissionRequestDTO request)
        {
            var result =
                await _service.AddPermissionsAsync(
                    id,
                    request.PermissionIds);

            return this.ApiOk(
                result,
                "Gán permission cho role thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.RemoveRolePermissions)]
        [HttpDelete("{id}/permissions")]
        public async Task<ActionResult<ApiResponse<RolePermissionResponseDTO>>> RemovePermissions(
            int id,
            [FromBody] RolePermissionRequestDTO request)
        {
            var result =
                await _service.RemovePermissionsAsync(
                    id,
                    request.PermissionIds);

            return this.ApiOk(
                result,
                "Xóa permission khỏi role thành công"
            );
        }

    }
}
