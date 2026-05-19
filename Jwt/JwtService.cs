using Demo_Course_Management.DTOs;
using Demo_Course_Management.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Demo_Course_Management.Jwt
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        //access token 
        public string GenerateAccessToken(User user)
        {
            //tạo private key lấy từ config 
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            //cấu hình jwt 
            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            // lấy roles từ UserRoles
            var roleNames = user.UserRoles
                .Select(x => x.Role.Name.ToString())
                .Distinct()
                .ToList();

            var claims = new List<Claim>
            {
                // user info
                new Claim("userId", user.Id.ToString()),
                new Claim("username", user.Username),

                //lưu ý cần được cấu hình và sử dụng
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                //thời điểm tạo accesstoken
                new Claim(JwtRegisteredClaimNames.Iat,DateTimeOffset.UtcNow
                        .ToUnixTimeSeconds()
                        .ToString(),
                    ClaimValueTypes.Integer64)
            };
            // roles
            foreach (var role in roleNames)
            {
                claims.Add(
                    new Claim(ClaimTypes.Role, role)
                );
            }

            //thời gian hết hạn access token
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

        //refresh token dùng random string 
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            return Convert.ToBase64String(randomBytes);
        }
    }
}
