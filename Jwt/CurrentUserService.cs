
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ShopManagementAPI.Jwt
    {
        //Claims dùng để đọc user hiện tại
        public class CurrentUserService
        {
            //inject từ Claims đang đăng nhập 
            private readonly IHttpContextAccessor _httpContextAccessor;

            public CurrentUserService(
                IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;
            }

            private ClaimsPrincipal? User =>
                _httpContextAccessor.HttpContext?.User;
            //user id
            public int UserId =>
                int.TryParse(
                    User?.FindFirst("userId")?.Value,
                    out var id
                )
                    ? id
                    : 0;
            //username
            public string Username =>
                User?.FindFirst("username")?.Value
                ?? string.Empty;

            // jti
            public string Jti =>
                User?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value
                ?? string.Empty;

            // issued at
            public long IssuedAt =>
                long.TryParse(
                    User?.FindFirst(JwtRegisteredClaimNames.Iat)?.Value,
                    out var iat
                )
                    ? iat
                    : 0;
            // expired at
            public string ExpiredAtString =>
                User?.FindFirst(JwtRegisteredClaimNames.Exp)?.Value
                ?? string.Empty;
           
            //roles 
            public List<string> Roles =>
                        User?.FindAll(ClaimTypes.Role)
                            .Select(x => x.Value)
                            .ToList()
                        ?? new List<string>();

            //check authenticated
            public bool IsAuthenticated =>
                User?.Identity?.IsAuthenticated ?? false;
        }
    }