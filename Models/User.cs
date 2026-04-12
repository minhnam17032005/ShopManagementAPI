using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo_Course_Management.Models
{
    [Table("Users")]
    public class User : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!; // tài khoản đăng nhập

        [Required]
        public string PasswordHash { get; set; } = null!; // hash mật khẩu

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!; // họ tên

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = null!; // email

        [Required]
        public int RoleId { get; set; } // FK → Role

        public Role Role { get; set; } = null!; // navigation property

        public List<Order> Orders { get; set; } = new();
    }
}
