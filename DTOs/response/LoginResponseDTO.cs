using ShopManagementAPI.Models;
using ShopManagementAPI.Models.Enum;

namespace ShopManagementAPI.DTOs.response
{
    public class LoginResponseDTO
    {
        public UserInfoDTO User { get; set; } = new();

        public string AccessToken { get; set; }
    }
}
