using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo_Course_Management.Models
{
    [Table("OrderItems")]
    public class OrderItem : BaseEntity
    {
        [Required]
        public int OrderId { get; set; } // FK → Order

        public Order Order { get; set; } = null!; // Navigation property

        [Required]
        public int ProductId { get; set; } // FK → Product

        public Product Product { get; set; } = null!; // Navigation property

        [Required]
        public int Quantity { get; set; } // số lượng

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // giá tại thời điểm mua
    }
}