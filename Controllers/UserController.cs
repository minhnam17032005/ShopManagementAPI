using Azure;
using ShopManagementAPI.Models;
using ShopManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementAPI.Authorization;
using ShopManagementAPI.DTOs.Common;
using ShopManagementAPI.Extensions;
using ShopManagementAPI.DTOs.request.User;
using ShopManagementAPI.DTOs.response.User;
using ShopManagementAPI.DTOs.response;

namespace ShopManagementAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly UserService _service;

        public UserController(UserService service)
        {
            _service = service;
        }

        [Authorize]
        [RequirePermission(Permissions.CreateUser)]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> Create(
    [FromBody] CreateUserReqDTO dto)
        {
            var result = await _service.CreateAsync(dto);

            return this.ApiCreated(
                result,
                "Tạo người dùng thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.UpdateUserProfile)]
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> ChangeProfile(
            ChangeProfileReqDTO dto)
        {
            var result = await _service.ChangeProfileAsync(dto);

            return this.ApiOk(
                result,
                "Cập nhật thông tin cá nhân thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.AddUserRoles)]
        [HttpPost("{id}/roles")]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> AddRoles(
            int id,
            [FromBody] ChangeRolesReqDTO dto)
        {
            var result = await _service.AddRolesAsync(
                id,
                dto.RoleIds);

            return this.ApiOk(
                result,
                "Thêm vai trò cho người dùng thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.RemoveUserRoles)]
        [HttpDelete("{id}/roles")]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> RemoveRoles(
            int id,
            [FromBody] ChangeRolesReqDTO dto)
        {
            var result = await _service.RemoveRolesAsync(
                id,
                dto.RoleIds);

            return this.ApiOk(
                result,
                "Xóa vai trò khỏi người dùng thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.GetUsers)]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponseDTO<UserResponseDTO>>>> GetAll(
        [FromQuery] UserQueryDTO request)
        {
            var result = await _service.GetAllAsync(request);

            return this.ApiOk(
                result,
                "Lấy danh sách người dùng thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.GetUserDetail)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserResponseDTO>>> GetById(
            int id)
        {
            var result = await _service.GetByIdAsync(id);

            return this.ApiOk(
                result,
                "Lấy chi tiết người dùng thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.LockUser)]
        [HttpPatch("{id}/lock")]
        public async Task<ActionResult<ApiResponse<StatusResponseDTO>>> Lock(
            int id)
        {
            var result = await _service.LockAsync(id);

            return this.ApiOk(
                result,
                "Khóa tài khoản thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.UnlockUser)]
        [HttpPatch("{id}/unlock")]
        public async Task<ActionResult<ApiResponse<StatusResponseDTO>>> Unlock(
            int id)
        {
            var result = await _service.UnlockAsync(id);

            return this.ApiOk(
                result,
                "Mở khóa tài khoản thành công"
            );
        }
    }
}
