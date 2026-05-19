using Demo_Course_Management.DTOs.request;
using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo_Course_Management.Controllers
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
        [HttpGet]
        public async Task<ActionResult<List<RoleResponseDTO>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleResponseDTO>> GetById(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        [Authorize]
        [HttpPost("{id}/permissions")]
        public async Task<ActionResult<RolePermissionResponseDTO>> AddPermissions(int id,[FromBody] RolePermissionRequestDTO request)
        {
            return Ok(await _service.AddPermissionsAsync(id, request.PermissionIds));
        }

        [Authorize]
        [HttpDelete("{id}/permissions")]
        public async Task<ActionResult<RolePermissionResponseDTO>> RemovePermissions(int id,[FromBody] RolePermissionRequestDTO request)
        {
            return Ok(await _service.RemovePermissionsAsync(id, request.PermissionIds));
        }




    }
}
