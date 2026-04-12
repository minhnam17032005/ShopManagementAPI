using Demo_Course_Management.DTOs.request;
using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.Models;
using Demo_Course_Management.Services;
using Microsoft.AspNetCore.Mvc;

namespace Demo_Course_Management.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _service;

        public CategoryController(CategoryService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<CategoryResponseDTO>> Create([FromBody] CategoryRequestDTO dto)
        {
            var category = await _service.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetById),       // action GetById trong controller
                new { id = category.Id }, // route values
                category                // body trả về
            );
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryResponseDTO>> Update([FromRoute] int id, [FromBody] CategoryRequestDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoryResponseDTO>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryResponseDTO>> GetById(int id)
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
    }
}
