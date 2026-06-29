using System.ComponentModel.DataAnnotations;

namespace ShopManagementAPI.DTOs.request.Auth
{
    public class RegisterRequestDTO
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = null!;
    }
}
