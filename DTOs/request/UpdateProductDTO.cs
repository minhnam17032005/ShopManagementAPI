using System.ComponentModel.DataAnnotations;

namespace Demo_Course_Management.DTOs.request
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
