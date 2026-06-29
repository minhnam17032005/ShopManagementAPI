namespace ShopManagementAPI.DTOs.response.Role
{
    public class RolePermissionResponseDTO
    {
        public int RoleId { get; set; }
        public List<int> ProcessedIds { get; set; } = new();
        public List<int> FailedIds { get; set; } = new();

        public string Message { get; set; }
    }
}
