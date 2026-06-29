using ShopManagementAPI.Exceptions;
using ShopManagementAPI.Models;

namespace ShopManagementAPI.Validators
{
    public static class OtpValidator
    {
        public static void Validate(OtpBaseEntity otp)
        {
            if (otp.UsedAt != null)
            {
                throw new BadRequestException(
                    "OTP đã được sử dụng.");
            }

            if (otp.RevokedAt != null)
            {
                throw new BadRequestException(
                    "OTP đã bị thu hồi.");
            }

            if (otp.ExpiredAt <= DateTime.UtcNow)
            {
                throw new BadRequestException(
                    "OTP đã hết hạn.");
            }
        }
    }
}
