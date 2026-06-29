using ShopManagementAPI.DTOs.Common;
using ShopManagementAPI.Models.Enum;

namespace ShopManagementAPI.DTOs.request.Order
{
    public class OrderQueryDTO : BaseQueryDTO
    {
        public int? UserId { get; set; }

        public OrderStatus? Status { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public decimal? MinTotalAmount { get; set; }

        public decimal? MaxTotalAmount { get; set; }
    }
}
