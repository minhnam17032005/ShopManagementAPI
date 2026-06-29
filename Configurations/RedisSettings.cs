namespace ShopManagementAPI.Configurations
{
    public class RedisSettings
    {
        public string ConnectionString { get; set; } = null!;

        public int PermissionCacheExpirationHours { get; set; }
    }
}
