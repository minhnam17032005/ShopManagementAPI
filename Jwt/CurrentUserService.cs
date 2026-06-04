
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ShopManagementAPI.Jwt
{
    // service lấy thông tin user từ jwt claims
    public class CurrentUserService
    {
        // inject ihttpcontextaccessor để truy cập httpcontext
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // claims của user hiện tại
        private ClaimsPrincipal? User =>
            _httpContextAccessor.HttpContext?.User;

        // user id
        public int UserId =>
            int.TryParse(
                User?.FindFirst("userId")?.Value,
                out var id
            )
                ? id
                : 0;

        // username
        public string Username =>
            User?.FindFirst("username")?.Value
            ?? string.Empty;

        // jwt id (unique token id)
        public string Jti =>
            User?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value
            ?? string.Empty;

        // issued at (thời gian tạo token)
        public long IssuedAt =>
            long.TryParse(
                User?.FindFirst(JwtRegisteredClaimNames.Iat)?.Value,
                out var iat
            )
                ? iat
                : 0;

        // expiration time (thời gian hết hạn ) dạng string 
        public string ExpiredAtString =>
            User?.FindFirst(JwtRegisteredClaimNames.Exp)?.Value
            ?? string.Empty;

        // danh sách roles của user
        public List<string> Roles =>
            User?.FindAll(ClaimTypes.Role)
                .Select(x => x.Value)
                .ToList()
            ?? new List<string>();

        // kiểm tra user đã đăng nhập chưa
        public bool IsAuthenticated =>
            User?.Identity?.IsAuthenticated ?? false;
    }
}