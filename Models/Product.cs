using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo_Course_Management.Models
{
    [Table("Products")]
    public class Product : BaseEntity
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

        public Category Category { get; set; } = null!; // Navigation property

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
