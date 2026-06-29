using System.ComponentModel.DataAnnotations;

namespace ShopManagementAPI.DTOs.request.User
{
    public class ChangeProfileReqDTO
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;
    }
}
