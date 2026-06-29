namespace ShopManagementAPI.DTOs.request.Auth
{
    public class ChangePasswordRequest
    {
        public string VerificationToken { get; set; } = default!;

        public string CurrentPassword { get; set; } = default!;

        public string NewPassword { get; set; } = default!;

        public string ConfirmPassword { get; set; } = default!;
    }
}
