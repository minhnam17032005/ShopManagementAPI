using ShopManagementAPI.DTOs.request;
using ShopManagementAPI.DTOs.response;
using ShopManagementAPI.Models;
using ShopManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementAPI.Authorization;
using ShopManagementAPI.DTOs.Common;
using ShopManagementAPI.Extensions;

namespace ShopManagementAPI.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Produces("application/json")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _service;

        public CategoryController(CategoryService service)
        {
            _service = service;
        }

        [Authorize]
        [RequirePermission(Permissions.CreateCategory)]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryResponseDTO>>> Create(
    [FromBody] CategoryRequestDTO dto)
        {
            var category = await _service.CreateAsync(dto);

            return this.ApiCreated(
                category,
                "Tạo danh mục thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.UpdateCategory)]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryResponseDTO>>> Update(
            [FromRoute] int id,
            [FromBody] CategoryRequestDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            return this.ApiOk(
                result,
                "Cập nhật danh mục thành công"
            );
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CategoryResponseDTO>>>> GetAll()
        {
            var result = await _service.GetAllAsync();

            return this.ApiOk(
                result,
                "Lấy danh sách danh mục thành công"
            );
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryResponseDTO>>> GetById(
            int id)
        {
            var result = await _service.GetByIdAsync(id);

            return this.ApiOk(
                result,
                "Lấy chi tiết danh mục thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.DeleteCategory)]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<StatusResponseDTO>>> Delete(
     int id)
        {
            var result = await _service.DeleteAsync(id);

            return this.ApiOk(
                result,
                "Xóa danh mục thành công"
            );
        }

        [Authorize]
        [RequirePermission(Permissions.RestoreCategory)]
        [HttpPatch("{id}/restore")]
        public async Task<ActionResult<ApiResponse<StatusResponseDTO>>> Restore(
            int id)
        {
            var result = await _service.RestoreAsync(id);

            return this.ApiOk(
                result,
                "Khôi phục danh mục thành công"
            );
        }
    }
}
