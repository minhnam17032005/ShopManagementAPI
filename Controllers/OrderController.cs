using ShopManagementAPI.DTOs.request;
using ShopManagementAPI.DTOs.response;
using ShopManagementAPI.Models;
using ShopManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementAPI.Authorization;

namespace ShopManagementAPIControllers
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
        [RequirePermission(Permissions.CreateOrder)]
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

        //dành cho management
        [Authorize]
        [RequirePermission(Permissions.GetOrders)]
        [HttpGet]
        public async Task<ActionResult<List<OrderResponseDTO>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        [Authorize]
        [RequirePermission(Permissions.GetOrderDetail)]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDTO>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }

        //dành cho customer
        [Authorize]
        [RequirePermission(Permissions.GetMyOrders)]
        [HttpGet("my-orders")]
        public async Task<ActionResult<List<OrderResponseDTO>>> GetMyOrders()
        {
            var result = await _service.GetMyOrdersAsync();
            return Ok(result);
        }   
        [Authorize]
        [RequirePermission(Permissions.GetMyOrderDetail)]
        [HttpGet("my-orders/{id}")]
        public async Task<ActionResult<OrderResponseDTO>> GetMyOrderById(int id)
        {
            var result = await _service.GetMyOrderByIdAsync(id);
            return Ok(result);
        }

        [Authorize]
        [RequirePermission(Permissions.UpdateOrderStatus)]
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<OrderResponseDTO>> UpdateStatus(int id, UpdateOrderStatusReqDTO dto)
        {
            var result = await _service.UpdateStatusAsync(id, dto);
            return Ok(result);
        }

        [Authorize]
        [RequirePermission(Permissions.CancelOrder)]
        [HttpPatch("{id}/cancel")]
        public async Task<ActionResult<OrderResponseDTO>> CancelOrder(int id)
        {
            var result = await _service.CancelOrderAsync(id);

            return Ok(result);
        }
    }
}
