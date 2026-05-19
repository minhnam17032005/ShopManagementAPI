
using System.Security.Claims;

namespace Demo_Course_Management.Jwt
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

            public int UserId =>
                int.TryParse(
                    User?.FindFirst("userId")?.Value,
                    out var id
                )
                    ? id
                    : 0;

            public string Username =>
                User?.FindFirst("username")?.Value
                ?? string.Empty;

            public List<string> Roles =>
                User?.FindAll(ClaimTypes.Role)
                    .Select(x => x.Value)
                    .ToList()
                ?? new List<string>();

            public bool IsAuthenticated =>
                User?.Identity?.IsAuthenticated ?? false;
        }
    }