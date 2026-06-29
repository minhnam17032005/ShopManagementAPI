using System.ComponentModel.DataAnnotations;
using ShopManagementAPI.Models.Enum;

namespace ShopManagementAPI.DTOs.response.Order
{
    public class OrderResponseDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string ReceiverName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string ShippingAddress { get; set; } = string.Empty;

        public string? Note { get; set; }
        public OrderStatus Status { get; set; }

        public decimal TotalAmount { get; set; }

        public List<OrderItemResponseDTO> Items { get; set; } = new();

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
