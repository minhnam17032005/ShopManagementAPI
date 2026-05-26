using ShopManagementAPI.DTOs.request;
using ShopManagementAPI.DTOs.response;
using ShopManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ShopManagementAPI.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Produces("application/json")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _service;

        public ProductController(ProductService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ProductResponseDTO>> Create([FromBody] ProductRequestDTO dto)
        {
            var product = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),           // tên action getById trong controller
                new { id = product.Id },   // route values
                product                    // body trả về
            );
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<ProductResponseDTO>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponseDTO>> GetById(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductResponseDTO>> Update(int id, UpdateProductDTO dto)
        {
            return Ok(await _service.UpdateAsync(id, dto));
        }

        [Authorize]
        [HttpPatch("{id}/stock")]
        public async Task<ActionResult<ProductResponseDTO>> UpdateStock(
            int id,
            UpdateStockDTO dto)
        {
            return Ok(await _service.UpdateStockAsync(id, dto));
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<StatusResponseDTO>> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return Ok(response);
        }

        [Authorize]
        [HttpPatch("{id}/restore")]
        public async Task<ActionResult<StatusResponseDTO>> Restore(int id)
        {
            var response =  await _service.RestoreAsync(id);
            return Ok(response);
        }
    }
}
