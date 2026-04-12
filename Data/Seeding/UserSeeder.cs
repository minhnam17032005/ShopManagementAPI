using Demo_Course_Management.Models;
using Demo_Course_Management.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace Demo_Course_Management.Data.Seeding
{
    public static class UserSeeder
    {
        public static object BCrypt { get; private set; }

        /*public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == RoleType.ADMIN);

            if (adminRole == null) return;

            var adminUser = new User
            {
                Username = "Nam",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),//LỖI 
                FullName = "Nguyen Minh Nam",
                Email = "namcoder2005@gmail.com",
                RoleId = adminRole.Id,
                CreatedAt = DateTime.Now
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }*/
    }
}
