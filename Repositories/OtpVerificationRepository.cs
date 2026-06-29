using Microsoft.EntityFrameworkCore;
using ShopManagementAPI.Models.Enum;
using ShopManagementAPI.Models;
using ShopManagementAPI.Data;

namespace ShopManagementAPI.Repositories
{
    public class OtpVerificationRepository
    {
        private readonly AppDbContext _context;

        public OtpVerificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(OtpVerification entity)
        {
            await _context.OtpVerifications.AddAsync(entity);
        }
        public async Task<OtpVerification?> GetValidAsync(
            int userId,
            string tokenHash,
            OtpType type)
        {
            return await _context.OtpVerifications
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.Type == type &&
                    x.TokenHash == tokenHash &&
                    x.UsedAt == null &&
                    x.RevokedAt == null);
        }

        public async Task<List<OtpVerification>> GetActiveTokensAsync(
            int userId,
            OtpType type)
        {
            return await _context.OtpVerifications
                .Where(x =>
                    x.UserId == userId &&
                    x.Type == type &&
                    x.UsedAt == null &&
                    x.RevokedAt == null &&
                    x.ExpiredAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<OtpVerification?> GetValidByTokenAsync(
            string tokenHash,
            OtpType type)
        {
            return await _context.OtpVerifications
                .FirstOrDefaultAsync(x =>
                    x.TokenHash == tokenHash &&
                    x.Type == type &&
                    x.UsedAt == null &&
                    x.RevokedAt == null &&
                    x.ExpiredAt > DateTime.UtcNow);
        }
    }
}
