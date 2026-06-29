using System.Security.Cryptography;

namespace ShopManagementAPI.Helpers
{
    //Sinh ra chuỗi 64 ký tự hex, đủ mạnh để làm OtpToken.
    public static class TokenGenerator
    {
        public static string GenerateSecureToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        }
    }
}
