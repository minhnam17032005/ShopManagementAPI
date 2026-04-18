using System.ComponentModel.DataAnnotations;

namespace Demo_Course_Management.DTOs.request
{
    public class CreateOrderReqDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public List<CreateOrderItemDTO> Items { get; set; } = new();
    }
}
