using Demo_Course_Management.DTOs.request;
using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.Services;
using Microsoft.AspNetCore.Mvc;

namespace Demo_Course_Management.Controllers
{
        [ApiController]
        [Route("api/orders")]
        public class OrderController : Controller
        {
            private readonly OrderService _service;

            public OrderController(OrderService service)
            {
                _service = service;
            }

        [HttpPost]
        public async Task<ActionResult<OrderResponseDTO>> Create(CreateOrderReqDTO dto)
        {
             var result = await _service.CreateAsync(dto);
             return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderResponseDTO>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDTO>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<OrderResponseDTO>> UpdateStatus(int id, UpdateOrderStatusReqDTO dto)
        {
            var result = await _service.UpdateStatusAsync(id, dto);
            return Ok(result);
        }
    }
}
