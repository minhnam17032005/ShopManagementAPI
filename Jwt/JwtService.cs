using ShopManagementAPI.DTOs;
using ShopManagementAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ShopManagementAPI.Jwt
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateAccessToken(User user)
        {
            // JWT signing key từ config
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            // Signing credentials (HMAC SHA256)
            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            // Lấy danh sách role của user
            var roleNames = user.UserRoles
                .Select(x => x.Role.Name.ToString())
                .Distinct()
                .ToList();

            var claims = new List<Claim>
            {
                // Thông tin user
                new Claim("userId", user.Id.ToString()),
                new Claim("username", user.Username),

                // Token ID (unique)
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                // Thời gian tạo token
                new Claim(JwtRegisteredClaimNames.Iat,DateTimeOffset.UtcNow
                        .ToUnixTimeSeconds()
                        .ToString(),
                    ClaimValueTypes.Integer64)
            };
            // Gán roles vào token
            foreach (var role in roleNames)
            {
                claims.Add(
                    new Claim(ClaimTypes.Role, role)
                );
            }

            // Thời gian hết hạn token
            var expires = DateTime.UtcNow.AddMinutes(
                int.Parse(_config["Jwt:AccessTokenExpirationMinutes"]!)
            );

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            // Random 64-byte token
            var randomBytes = new byte[64];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            // Encode sang Base64
            return Convert.ToBase64String(randomBytes);
        }
    }
}
