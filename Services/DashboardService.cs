using ShopManagementAPI.DTOs.response.Dashboard;
using ShopManagementAPI.Models.Enum;
using ShopManagementAPI.Repositories;

namespace ShopManagementAPI.Services
{
    public class DashboardService
    {
        private readonly ProductRepository _repoProduct;
        private readonly OrderRepository _repoOrder;

        public DashboardService(ProductRepository repoProduct,OrderRepository repoOrder)
        {
            _repoProduct = repoProduct;
            _repoOrder = repoOrder;
        }
        //Dashboard tổng quan
        public async Task<DashboardOverviewDTO> GetOverviewAsync()
        {   
            return new DashboardOverviewDTO
            {
                // Tổng sản phẩm
                TotalProducts = await _repoProduct.CountAsync(),

                // Tổng đơn hàng
                TotalOrders = await _repoOrder.CountAsync(),

                // Đơn chờ xác nhận
                PendingOrders = await _repoOrder.CountByStatusAsync(OrderStatus.PENDING),

                // Đơn đang giao
                ShippingOrders = await _repoOrder.CountByStatusAsync(OrderStatus.SHIPPING),

                // Đơn hoàn thành
                CompletedOrders = await _repoOrder.CountByStatusAsync(OrderStatus.COMPLETED)
            };
        }

        //Dashboard doanh thu
        public async Task<RevenueDashboardDTO> GetRevenueAsync()
        {
            return new RevenueDashboardDTO
            {
                // Tổng doanh thu từ đơn hoàn thành
                TotalRevenue = await _repoOrder.GetTotalRevenueAsync()
            };
        }
    }
}
