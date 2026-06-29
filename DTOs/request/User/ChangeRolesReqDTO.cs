using System.ComponentModel.DataAnnotations;

namespace ShopManagementAPI.DTOs.request.User
{
    public class ChangeRolesReqDTO
    {
        [Required]
        [MinLength(1)]
        public List<int> RoleIds { get; set; } = new();
    }
}
