using Demo_Course_Management.Models.Enum;
using Demo_Course_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_Course_Management.Data.Seeding
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            //lấy tất cả role đã có
            var existingRoleTypes = await context.Roles
                .Select(r => r.Name)
                .ToListAsync();

            var rolesToSeed = new List<Role>
            {
                new Role { Name = RoleType.ADMIN, Description = "System Admin" },
                new Role { Name = RoleType.STAFF, Description = "Staff" },
                new Role { Name = RoleType.CUSTOMER, Description = "Customer" }
            };

            //lọc những role chưa có
            var toAdd = rolesToSeed
                .Where(r => !existingRoleTypes.Contains(r.Name))
                .ToList();

            if (toAdd.Any())
            {
                context.Roles.AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }
    }
}
