using System.ComponentModel.DataAnnotations;

namespace ShopManagementAPI.DTOs.request.Auth
{
    public class VerifyChangePasswordOtpRequest
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = default!;
    }
}
