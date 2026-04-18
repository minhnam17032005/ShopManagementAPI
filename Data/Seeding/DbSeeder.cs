using Demo_Course_Management.Models;
using Demo_Course_Management.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace Demo_Course_Management.Data.Seeding
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await PermissionSeeder.SeedAsync(context);
            await RoleSeeder.SeedAsync(context);
            await RolePermissionSeeder.SeedAsync(context);
            await UserSeeder.SeedAsync(context);
        }
    }
}
