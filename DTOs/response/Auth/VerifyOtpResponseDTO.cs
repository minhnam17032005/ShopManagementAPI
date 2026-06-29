namespace ShopManagementAPI.DTOs.response.Auth
{
    public class VerifyOtpResponseDTO
    {
        public string VerificationToken { get; set; } = null!;

        public DateTime ExpiredAt { get; set; }
    }
}
