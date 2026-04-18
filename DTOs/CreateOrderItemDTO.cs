using System.ComponentModel.DataAnnotations;

namespace Demo_Course_Management.DTOs
{
    public class CreateOrderItemDTO
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }
    }
}
