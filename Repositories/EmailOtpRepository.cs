using Microsoft.EntityFrameworkCore;
using ShopManagementAPI.Models.Enum;
using ShopManagementAPI.Models;
using ShopManagementAPI.Data;

namespace ShopManagementAPI.Repositories
{
    public class EmailOtpRepository
    {
        private readonly AppDbContext _context;

        public EmailOtpRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<EmailOtp?> GetLatestValidAsync(
        int userId,
        OtpType type)
        {
            return await _context.EmailOtps
                .Where(x =>
                    x.UserId == userId &&
                    x.Type == type &&
                    x.UsedAt == null &&
                    x.RevokedAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<EmailOtp?> GetValidAsync(
            int userId,
            OtpType type)
        {
            return await _context.EmailOtps
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.Type == type &&
                    x.UsedAt == null &&
                    x.RevokedAt == null &&
                    x.ExpiredAt > DateTime.UtcNow);
        }
    }
}
