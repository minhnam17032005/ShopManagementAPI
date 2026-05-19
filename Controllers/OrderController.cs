using Demo_Course_Management.DTOs.request;
using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.Models;
using Demo_Course_Management.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo_Course_Management.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Produces("application/json")]
    public class OrderController : ControllerBase
        {
            private readonly OrderService _service;

            public OrderController(OrderService service)
            {
                _service = service;
            }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<OrderResponseDTO>> Create(CreateOrderReqDTO dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(
               nameof(GetById),       // action GetById trong controller
               new { id = result.Id }, // route values
               result                // body trả về
           );
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<OrderResponseDTO>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDTO>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }

        [Authorize]
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<OrderResponseDTO>> UpdateStatus(int id, UpdateOrderStatusReqDTO dto)
        {
            var result = await _service.UpdateStatusAsync(id, dto);
            return Ok(result);
        }
    }
}
