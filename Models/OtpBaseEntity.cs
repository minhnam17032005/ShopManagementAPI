using ShopManagementAPI.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace ShopManagementAPI.Models
{
    public abstract class OtpBaseEntity : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        // Email tại thời điểm tạo OTP/Verification
        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        // Lưu hash của giá trị xác thực.
        // EmailOtp: OTP
        // OtpVerification: Verification Token
        [Required]
        [MaxLength(255)]
        public string TokenHash { get; set; } = null!;

        // Mục đích sử dụng
        [Required]
        public OtpType Type { get; set; }

        [Required]
        public DateTime ExpiredAt { get; set; }

        public DateTime? UsedAt { get; set; }

        public DateTime? RevokedAt { get; set; }
    }
}
