using ShopManagementAPI.DTOs.request;
using ShopManagementAPI.DTOs.response;
using ShopManagementAPI.Models;
using ShopManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementAPI.Authorization;
using ShopManagementAPI.DTOs.Common;
using ShopManagementAPI.Extensions;

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
        public async Task<ActionResult<ApiResponse<OrderResponseDTO>>> Create(
    CreateOrderReqDTO dto)
        {
            var result = await _service.CreateAsync(dto);

            return this.ApiCreated(
                result,
                "Tạo đơn hàng thành công"
            );
        }

        // dành cho management
        [Authorize]
        [RequirePermission(Permissions.GetOrders)]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<OrderResponseDTO>>>> GetAll()
        {
            var result = await _service.GetAllAsync();

            return this.ApiOk(
                result,
                "Lấy danh sách đơn hàng thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.GetOrderDetail)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<OrderResponseDTO>>> GetById(
            int id)
        {
            var result = await _service.GetByIdAsync(id);

            return this.ApiOk(
                result,
                "Lấy chi tiết đơn hàng thành công"
            );
        }

        // dành cho customer
        [Authorize]
        [RequirePermission(Permissions.GetMyOrders)]
        [HttpGet("my-orders")]
        public async Task<ActionResult<ApiResponse<List<OrderResponseDTO>>>> GetMyOrders()
        {
            var result = await _service.GetMyOrdersAsync();

            return this.ApiOk(
                result,
                "Lấy danh sách đơn hàng của tôi thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.GetMyOrderDetail)]
        [HttpGet("my-orders/{id}")]
        public async Task<ActionResult<ApiResponse<OrderResponseDTO>>> GetMyOrderById(
            int id)
        {
            var result = await _service.GetMyOrderByIdAsync(id);

            return this.ApiOk(
                result,
                "Lấy chi tiết đơn hàng của tôi thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.UpdateOrderStatus)]
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponse<OrderResponseDTO>>> UpdateStatus(
            int id,
            UpdateOrderStatusReqDTO dto)
        {
            var result = await _service.UpdateStatusAsync(id, dto);

            return this.ApiOk(
                result,
                "Cập nhật trạng thái đơn hàng thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.CancelOrder)]
        [HttpPatch("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<OrderResponseDTO>>> CancelOrder(
            int id)
        {
            var result = await _service.CancelOrderAsync(id);

            return this.ApiOk(
                result,
                "Hủy đơn hàng thành công"
            );
        }
    }
}
