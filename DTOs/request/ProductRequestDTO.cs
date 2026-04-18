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
        [Range(1, int.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0.")]
        public decimal Price { get; set; } // Giá sản phẩm

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải lớn hơn 0.")]
        public int Stock { get; set; } // Số lượng tồn kho


        [Required]
        public int CategoryId { get; set; } // FK → Category
    }
}
