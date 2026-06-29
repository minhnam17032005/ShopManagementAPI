using System.Security.Cryptography;

namespace ShopManagementAPI.Helpers
{
    // Sinh mã OTP ngẫu nhiên gồm 6 chữ số
    public static class OtpGenerator
    {
        public static string GenerateOtp()
        {
            return RandomNumberGenerator
                .GetInt32(100000, 999999)
                .ToString();
        }
    }
}
