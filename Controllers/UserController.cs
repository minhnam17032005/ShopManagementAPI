using Demo_Course_Management.DTOs.request;
using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.Models;
using Demo_Course_Management.Services;
using Microsoft.AspNetCore.Mvc;

namespace Demo_Course_Management.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : Controller
    {
        private readonly UserService _service;

        public UserController(UserService service)
        {
            _service = service;
        }

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

        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponseDTO>> Update(int id, UpdateUserReqDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<List<UserResponseDTO>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDTO>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent(); // 204 chuẩn REST
        }

        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            await _service.RestoreAsync(id);
            return Ok(new { message = "Khôi phục tài khoản thành công." });
        }
    }
}
