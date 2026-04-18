using Demo_Course_Management.Models.Enum;

namespace Demo_Course_Management.DTOs.response
{
    public class OrderResponseDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public OrderStatus Status { get; set; }

        public decimal TotalAmount { get; set; }

        public List<OrderItemResponseDTO> Items { get; set; } = new();

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
