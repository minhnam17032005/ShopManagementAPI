using Demo_Course_Management.DTOs.request;
using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo_Course_Management.Controllers
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
        [HttpGet]
        public async Task<ActionResult<List<PermissionResponseDTO>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionResponseDTO>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }


    }
}
