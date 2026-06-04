using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ShopManagementAPI.Models.Enum;

namespace ShopManagementAPI.Models
{
    [Table("Orders")]
    public class Order : BaseEntity
    {
        [Required]
        public int UserId { get; set; } // FK → User

        public User User { get; set; } = null!; // navigation property

        [Required]
        public string ReceiverName { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string ShippingAddress { get; set; } = string.Empty;

        public string? Note { get; set; }

        [Required]
        public OrderStatus Status { get; set; } // enum Pending, Completed, Cancelled

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } // tổng tiền

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
