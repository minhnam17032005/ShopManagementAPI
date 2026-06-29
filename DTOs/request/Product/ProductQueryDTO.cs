using ShopManagementAPI.DTOs.Common;

namespace ShopManagementAPI.DTOs.request.Product
{
    public class ProductQueryDTO : BaseQueryDTO
    {
        public bool? IsActive { get; set; }

        public int? CategoryId { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }
    }
}
