using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Demo_Course_Management.DTOs.request
{
    public class ProductRequestDTO
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!; // Tên sản phẩm

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Giá sản phẩm

        [Required]
        public int Stock { get; set; } // Số lượng tồn kho

        [Required]
        public int CategoryId { get; set; } // FK → Category
    }
}
