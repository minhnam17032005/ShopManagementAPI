using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopManagementAPI.Models
{
    [Table("Users")]
    public class User : BaseEntity
    {
        [Required, MaxLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required, MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        //Refresh Token (simple version - 1 device)
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiredAt { get; set; }

        public List<UserRole> UserRoles { get; set; } = new();
        public List<Order> Orders { get; set; } = new();

        public List<EmailOtp> EmailOtps { get; set; } = new();

        public List<OtpVerification> OtpVerifications { get; set; } = new();
    }
}
