namespace ShopManagementAPI.DTOs
{
    public class UserInfoDTO
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public List<RoleItemDTO> Roles { get; set; } = new();
    }
}
