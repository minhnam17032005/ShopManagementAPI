using ShopManagementAPI.Models.Enum;

namespace ShopManagementAPI.DTOs.request.Auth
{
    public class ForgotPasswordOtpRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}
