using ShopManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopManagementAPI.Authorization;
using ShopManagementAPI.DTOs.Common;
using ShopManagementAPI.Extensions;
using ShopManagementAPI.DTOs.request.Product;
using ShopManagementAPI.DTOs.response.Product;
using ShopManagementAPI.DTOs.response;

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
        [RequirePermission(Permissions.CreateProduct)]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductResponseDTO>>> Create(
    [FromBody] ProductRequestDTO dto)
        {
            var product = await _service.CreateAsync(dto);

            return this.ApiCreated(
                product,
                "Tạo sản phẩm thành công"
            );
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponseDTO<ProductResponseDTO>>>> GetAll(
        [FromQuery] ProductQueryDTO request)
        {
            var result = await _service.GetAllAsync(request);

            return this.ApiOk(
                result,
                "Lấy danh sách sản phẩm thành công"
            );
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductResponseDTO>>> GetById(
            int id)
        {
            var result = await _service.GetByIdAsync(id);

            return this.ApiOk(
                result,
                "Lấy chi tiết sản phẩm thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.UpdateProduct)]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ProductResponseDTO>>> Update(
            int id,
            UpdateProductDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            return this.ApiOk(
                result,
                "Cập nhật sản phẩm thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.UpdateProductStock)]
        [HttpPatch("{id}/stock")]
        public async Task<ActionResult<ApiResponse<ProductResponseDTO>>> UpdateStock(
            int id,
            UpdateStockDTO dto)
        {
            var result = await _service.UpdateStockAsync(id, dto);

            return this.ApiOk(
                result,
                "Cập nhật tồn kho thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.DeleteProduct)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<StatusResponseDTO>>> Delete(
    int id)
        {
            var result = await _service.DeleteAsync(id);

            return this.ApiOk(
                result,
                "Xóa sản phẩm thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.RestoreProduct)]
        [HttpPatch("{id}/restore")]
        public async Task<ActionResult<ApiResponse<StatusResponseDTO>>> Restore(
            int id)
        {
            var result = await _service.RestoreAsync(id);

            return this.ApiOk(
                result,
                "Khôi phục sản phẩm thành công"
            );
        }
    }
}
