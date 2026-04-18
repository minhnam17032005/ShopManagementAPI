namespace Demo_Course_Management.DTOs
{
    public class OrderItemResponseDTO
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = null!;

        public int Quantity { get; set; }

        public decimal Price { get; set; }
    }
}
