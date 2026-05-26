using StackExchange.Redis;

namespace ShopManagementAPI.Jwt
{
    public class JwtBlacklistService
    {
        private readonly IDatabase _redis;

        public JwtBlacklistService(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        // blacklist token
        public async Task BlacklistTokenAsync(string jti, TimeSpan ttl)
        {
            await _redis.StringSetAsync(
                $"blacklist:{jti}",
                "revoked",
                ttl
            );
        }

        // check token bị blacklist chưa
        public async Task<bool> IsBlacklistedAsync(string jti)
        {
            return await _redis.KeyExistsAsync(
                $"blacklist:{jti}"
            );
        }
    }
}