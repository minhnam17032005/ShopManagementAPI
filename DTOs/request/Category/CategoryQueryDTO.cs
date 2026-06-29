using ShopManagementAPI.DTOs.Common;

namespace ShopManagementAPI.DTOs.request.Category
{
    public class CategoryQueryDTO : BaseQueryDTO
    {
        public bool? IsActive { get; set; }
    }
}
