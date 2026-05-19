using Azure;
using Demo_Course_Management.DTOs.request;
using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.Models;
using Demo_Course_Management.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo_Course_Management.Controllers
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
        [HttpPost]
        public async Task<ActionResult<UserResponseDTO>> Create([FromBody] CreateUserReqDTO dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetById),           
                new { id = result.Id },
                result
            );
        }

        [Authorize]
        [HttpPut("{id}/profile")]
        public async Task<ActionResult<UserResponseDTO>> ChangeProfile(int id, ChangeProfileReqDTO dto)
        {
            var result = await _service.ChangeProfileAsync(id, dto);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("{id}/roles")]
        public async Task<ActionResult<UserResponseDTO>> AddRoles(
            int id,
            [FromBody] ChangeRolesReqDTO dto)
        {
            var result = await _service.AddRolesAsync(id, dto.RoleIds);

            return Ok(new
            {
                message = "Thêm vai trò cho người dùng thành công.",
                data = result
            });
        }

        [Authorize]
        [HttpDelete("{id}/roles")]
        public async Task<ActionResult<UserResponseDTO>> RemoveRoles(
            int id,
            [FromBody] ChangeRolesReqDTO dto)
        {
            var result = await _service.RemoveRolesAsync(id, dto.RoleIds);

            return Ok(new {
                message = "Xóa vai trò khỏi người dùng thành công.",
                data = result
            });
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<UserResponseDTO>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDTO>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }

        [Authorize]
        [HttpPatch("{id}/lock")]
        public async Task<ActionResult<StatusResponseDTO>> Lock(int id)
        {
            var response = await _service.LockAsync(id);
            return Ok(response);
        }

        [Authorize]
        [HttpPatch("{id}/unlock")]
        public async Task<ActionResult<StatusResponseDTO>> Unlock(int id)
        {
            var response = await _service.UnlockAsync(id);
            return Ok(response);
        }
    }
}
