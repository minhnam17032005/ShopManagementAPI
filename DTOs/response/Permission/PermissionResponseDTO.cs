using ShopManagementAPI.Models.Enum;

namespace ShopManagementAPI.DTOs.response.Permission
{
    public class PermissionResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ApiPath { get; set; }
        public HttpMethodType Method { get; set; }
        public string Module { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
