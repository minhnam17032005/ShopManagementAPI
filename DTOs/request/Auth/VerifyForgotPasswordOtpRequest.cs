using System.ComponentModel.DataAnnotations;

namespace ShopManagementAPI.DTOs.request.Auth
{
    public class VerifyForgotPasswordOtpRequest
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = default!;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = null!;
    }
}
