using Demo_Course_Management.Models.Enum;
using Demo_Course_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_Course_Management.Data.Seeding
{
    public static class RolePermissionSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var roles = await context.Roles.ToListAsync();
            var permissions = await context.Permissions.ToListAsync();

            var admin = roles.FirstOrDefault(r => r.Name == RoleType.ADMIN);
            var staff = roles.FirstOrDefault(r => r.Name == RoleType.STAFF);
            var customer = roles.FirstOrDefault(r => r.Name == RoleType.CUSTOMER);

            if (admin == null || staff == null || customer == null) return;

            //existing để tránh trùng
            var existing = await context.RolePermissions
                .Select(rp => new { rp.RoleId, rp.PermissionId })
                .ToListAsync();

            var toAdd = new List<RolePermission>();

            //ADMIN → full quyền
            foreach (var p in permissions)
            {
                if (!existing.Any(e => e.RoleId == admin.Id && e.PermissionId == p.Id))
                {
                    toAdd.Add(new RolePermission
                    {
                        RoleId = admin.Id,
                        PermissionId = p.Id
                    });
                }
            }

            //STAFF → Category + Product (GET, POST, PUT)
            var staffPermissions = permissions
                .Where(p =>
                    (p.Module == "Category" || p.Module == "Product") &&
                    (p.Method == HttpMethodType.GET ||
                     p.Method == HttpMethodType.POST ||
                     p.Method == HttpMethodType.PUT))
                .ToList();

            foreach (var p in staffPermissions)
            {
                if (!existing.Any(e => e.RoleId == staff.Id && e.PermissionId == p.Id))
                {
                    toAdd.Add(new RolePermission
                    {
                        RoleId = staff.Id,
                        PermissionId = p.Id
                    });
                }
            }

            //CUSTOMER → chỉ GET (Category + Product)
            var customerPermissions = permissions
                .Where(p =>
                    (p.Module == "Category" || p.Module == "Product") &&
                    p.Method == HttpMethodType.GET)
                .ToList();

            foreach (var p in customerPermissions)
            {
                if (!existing.Any(e => e.RoleId == customer.Id && e.PermissionId == p.Id))
                {
                    toAdd.Add(new RolePermission
                    {
                        RoleId = customer.Id,
                        PermissionId = p.Id
                    });
                }
            }

            if (toAdd.Any())
            {
                context.RolePermissions.AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }
    }
}
