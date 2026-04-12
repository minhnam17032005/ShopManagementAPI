using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Demo_Course_Management.Models.Enum;

namespace Demo_Course_Management.Models
{
    [Table("Orders")]
    public class Order : BaseEntity
    {
        [Required]
        public int UserId { get; set; } // FK → User

        public User User { get; set; } = null!; // navigation property

        [Required]
        public OrderStatus Status { get; set; } // enum Pending, Completed, Cancelled

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } // tổng tiền

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
