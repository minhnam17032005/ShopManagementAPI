using System.ComponentModel.DataAnnotations;

namespace ShopManagementAPI.DTOs.request.Product
{
    public class UpdateStockDTO
    {
        [Required]
        public int Stock { get; set; }
    }
}
