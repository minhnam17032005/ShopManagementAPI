using Demo_Course_Management.Models.Enum;

namespace Demo_Course_Management.DTOs.response
{
    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int RoleId { get; set; }

        public bool IsActive { get; set; }
        public RoleType RoleName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
