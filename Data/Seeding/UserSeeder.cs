using ShopManagementAPI.Middleware;
using ShopManagementAPI.Models;
using ShopManagementAPI.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace ShopManagementAPI.Data.Seeding
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var adminRole = await context.Roles.FirstAsync(x => x.Name == RoleType.ADMIN);
            
            //check xem đã có admin ch
            var hasAdmin = await context.Users
            .AnyAsync(x => x.UserRoles.Any(ur => ur.Role.Name == RoleType.ADMIN));
            if (hasAdmin)
                return;

            var user = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                FullName = "Super admin",
                Email = "admin2005@gmail.com",
                IsActive = true,
                UserRoles = new List<UserRole>
        {
            new UserRole { RoleId = adminRole.Id }
        }
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }
    }
}
