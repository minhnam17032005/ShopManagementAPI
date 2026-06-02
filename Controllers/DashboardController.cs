using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopManagementAPI.Authorization;
using ShopManagementAPI.DTOs.Common;
using ShopManagementAPI.DTOs.response;
using ShopManagementAPI.Extensions;
using ShopManagementAPI.Services;

namespace ShopManagementAPI.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    [Produces("application/json")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _service;

        public DashboardController(
            DashboardService service)
        {
            _service = service;
        }
        // Dashboard tổng quan
        [Authorize]
        [RequirePermission(Permissions.ViewDashboardOverview)]
        [HttpGet("overview")]
        public async Task<ActionResult<ApiResponse<DashboardOverviewDTO>>>GetOverview()
        {
            var result = await _service.GetOverviewAsync();

            return this.ApiOk(
                result,
                "Lấy dashboard tổng quan thành công"
            );
        }

        // Dashboard doanh thu
        [Authorize]
        [RequirePermission(Permissions.ViewRevenue)]
        [HttpGet("revenue")]
        public async Task<ActionResult<ApiResponse<RevenueDashboardDTO>>>GetRevenue()
        {
            var result = await _service.GetRevenueAsync();

            return this.ApiOk(
                result,
                "Lấy doanh thu thành công"
            );
        }
    }

}

