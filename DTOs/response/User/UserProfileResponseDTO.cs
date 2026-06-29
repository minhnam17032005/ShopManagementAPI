namespace ShopManagementAPI.DTOs.response.User
{
    public class UserProfileResponseDTO
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public List<RoleItemDTO> Roles { get; set; } = new();

        public List<string> Permissions { get; set; } = new();

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
