using ShopManagementAPI.Models.Enum;

namespace ShopManagementAPI.DTOs.response
{
    public class RoleResponseDTO
    {
        public int Id { get; set; }
        public RoleType Name { get; set; }
        public string Description { get; set; }
        public List<PermissionItemDTO> Permissions { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
