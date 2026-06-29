using System.Security.Cryptography;
using System.Text;

namespace ShopManagementAPI.Helpers
{
    // Hỗ trợ băm dữ liệu bằng SHA256
    public static class HashHelper
    {
        public static string Hash(string value)
        {
            using var sha = SHA256.Create();

            var bytes = sha.ComputeHash(
                Encoding.UTF8.GetBytes(value));

            return Convert.ToHexString(bytes);
        }

        // Kiểm tra dữ liệu gốc có khớp với hash OTP và Verification Token đã lưu hay không
        public static bool Verify(string value, string hash)
        {
            string computedHash = Hash(value);

            return CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(computedHash),
                Convert.FromHexString(hash));
        }
    }

}
