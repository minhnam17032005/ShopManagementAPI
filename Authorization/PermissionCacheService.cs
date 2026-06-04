using ShopManagementAPI.Repositories;
using StackExchange.Redis;
using System.Text.Json;

namespace ShopManagementAPI.Authorization
{
    public class PermissionCacheService
    {
        private readonly IDatabase _redis;
        private readonly UserRepository _userRepository;
        private readonly ILogger<PermissionCacheService> _logger;
        private readonly IConfiguration _config;
        private readonly IConnectionMultiplexer _multiplexer;

        public PermissionCacheService(UserRepository userRepository
                    , ILogger<PermissionCacheService> logger, IConfiguration config, IConnectionMultiplexer multiplexer)
        {
            _redis = multiplexer.GetDatabase();
            _multiplexer = multiplexer;
            _userRepository = userRepository;
            _logger = logger;
            _config = config;
        }

        public async Task<List<string>> GetPermissionsAsync(int userId)
        {
            // cache key theo user
            var cacheKey = $"permissions:{userId}";

            // lấy từ Redis
            var cachedPermissions =
                await _redis.StringGetAsync(cacheKey);

            // cache hit
            if (!cachedPermissions.IsNullOrEmpty){
                _logger.LogInformation(
                    "Permission cache HIT for user {UserId}",
                    userId);

                return JsonSerializer.Deserialize<List<string>>(
                    cachedPermissions!
                )!;
            }

            _logger.LogInformation(
                "Permission cache MISS for user {UserId}",
                userId);
            // query DB khi miss
            var user = await _userRepository
                    .GetUserWithRolesAndPermissionsAsync(userId);

            if (user == null){
                return new List<string>();
            }

            // lấy permissions từ roles
            var permissions = user.UserRoles
                .SelectMany(x => x.Role.RolePermissions)
                .Select(x => x.Permission.Name)
                .Distinct()
                .ToList();

            // lưu cache Redis
            await _redis.StringSetAsync(
                cacheKey,
                JsonSerializer.Serialize(permissions),
                TimeSpan.FromHours(
                int.Parse(
                    _config["Redis:PermissionCacheExpirationHours"]!)
                )
            );

            return permissions;
        }

        public async Task RemovePermissionsAsync(int userId)
        {
            // Xóa cache permission của user
            await _redis.KeyDeleteAsync($"permissions:{userId}");
        }

        // Xóa toàn bộ cache permissions
        public async Task ClearAllPermissionsCacheAsync()
        {
            // Lấy Redis server hiện tại
            var endpoint = _multiplexer.GetEndPoints().First();
            var server = _multiplexer.GetServer(endpoint);

            // Lấy tất cả key permissions:*
            var keys = server.Keys(pattern: "permissions:*");

            foreach (var key in keys)
            {
                await _redis.KeyDeleteAsync(key);
            }
        }
    }
}
