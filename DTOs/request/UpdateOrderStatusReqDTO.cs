using System.ComponentModel.DataAnnotations;
using Demo_Course_Management.Models.Enum;

namespace Demo_Course_Management.DTOs.request
{
    public class UpdateOrderStatusReqDTO
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}
