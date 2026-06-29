using System.ComponentModel.DataAnnotations;

namespace ShopManagementAPI.DTOs.request.Auth
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string VerificationToken { get; set; } = null!;

        [Required]
        public string NewPassword { get; set; } = null!;

        [Required]
        public string ConfirmPassword { get; set; } = null!;
    }
}
