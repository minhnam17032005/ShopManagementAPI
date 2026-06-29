using ShopManagementAPI.DTOs.Common;
using ShopManagementAPI.Models.Enum;

namespace ShopManagementAPI.DTOs.request.Permission
{
    public class PermissionQueryDTO : BaseQueryDTO
    {
        public string? Module { get; set; }

        public string? ApiPath { get; set; }

        public HttpMethodType? Method { get; set; }
    }
}
