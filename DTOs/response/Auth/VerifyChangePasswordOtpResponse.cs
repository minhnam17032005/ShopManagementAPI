namespace ShopManagementAPI.DTOs.response.Auth
{
    public class VerifyChangePasswordOtpResponse
    {
        public string OtpToken { get; set; } = default!;

        public DateTime ExpiredAt { get; set; }
    }
}
