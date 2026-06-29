using System.ComponentModel.DataAnnotations;

namespace ShopManagementAPI.DTOs.request.Product
{
    public class UpdateProductDTO
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}
