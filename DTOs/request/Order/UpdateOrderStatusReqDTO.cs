using System.ComponentModel.DataAnnotations;
using ShopManagementAPI.Models.Enum;

namespace ShopManagementAPI.DTOs.request.Order
{
    public class UpdateOrderStatusReqDTO
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}
