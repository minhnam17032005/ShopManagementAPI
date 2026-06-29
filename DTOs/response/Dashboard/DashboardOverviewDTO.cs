namespace ShopManagementAPI.DTOs.response.Dashboard
{
    public class DashboardOverviewDTO
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int CompletedOrders { get; set; }
    }
}
