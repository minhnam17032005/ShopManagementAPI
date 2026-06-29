using ShopManagementAPI.DTOs.Common;
using ShopManagementAPI.Models.Enum;

namespace ShopManagementAPI.DTOs.request.User
{
    public class UserQueryDTO : BaseQueryDTO
    {
        public bool? IsActive { get; set; }

        public RoleType? Role { get; set; }
    }
}
