using ShopManagementAPI.Models.Enum;

namespace ShopManagementAPI.DTOs.response.User
{
    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public List<RoleItemDTO> Roles { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
