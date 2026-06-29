using ShopManagementAPI.Models;
using ShopManagementAPI.Models.Enum;

namespace ShopManagementAPI.DTOs.response.Auth
{
    public class LoginResponseDTO
    {
        public UserInfoDTO User { get; set; } = new();

        public string AccessToken { get; set; }
    }
}
