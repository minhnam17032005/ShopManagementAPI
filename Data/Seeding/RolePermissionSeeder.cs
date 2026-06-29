using Microsoft.EntityFrameworkCore;
using ShopManagementAPI.Models;
using ShopManagementAPI.Models.Enum;

namespace ShopManagementAPI.Data.Seeding
{
    public static class RolePermissionSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var roles = await context.Roles.AsNoTracking().ToListAsync();
            var permissions = await context.Permissions.AsNoTracking().ToListAsync();

            var admin = roles.First(x => x.Name == RoleType.ADMIN);
            var manager = roles.First(x => x.Name == RoleType.MANAGER);
            var staff = roles.First(x => x.Name == RoleType.STAFF);
            var customer = roles.First(x => x.Name == RoleType.CUSTOMER);

            var toAdd = new List<RolePermission>();

            void AddIfNotExists(int roleId, int permissionId)
            {
                if (!context.RolePermissions.Any(x =>
                    x.RoleId == roleId &&
                    x.PermissionId == permissionId))
                {
                    toAdd.Add(new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permissionId
                    });
                }
            }

            // =========================
            // ADMIN
            // Toàn quyền
            // =========================
            foreach (var permission in permissions)
            {
                AddIfNotExists(admin.Id, permission.Id);
            }

            // =========================
            // MANAGER
            // Quản lý sản phẩm, danh mục, kho
            // Xem đơn hàng
            // Xem user
            // Dashboard + Revenue
            // =========================
            string[] managerPermissions =
            {
                // Category
                "CREATE_CATEGORY",
                "UPDATE_CATEGORY",
                "DELETE_CATEGORY",
                "RESTORE_CATEGORY",

                // Product
                "CREATE_PRODUCT",
                "UPDATE_PRODUCT",
                "DELETE_PRODUCT",
                "UPDATE_PRODUCT_STOCK",
                "RESTORE_PRODUCT",

                // Order
                "GET_ORDERS",
                "GET_ORDER_DETAIL",

                // User
                "GET_USERS",
                "GET_USER_DETAIL",

                // Dashboard
                "VIEW_DASHBOARD_OVERVIEW",
                "VIEW_REVENUE"
            };

            foreach (var permission in permissions
                .Where(x => managerPermissions.Contains(x.Name)))
            {
                AddIfNotExists(manager.Id, permission.Id);
            }

            // =========================
            // STAFF
            // Xử lý đơn hàng
            // =========================
            string[] staffPermissions =
            {
                "GET_ORDERS",
                "GET_ORDER_DETAIL",
                "UPDATE_ORDER_STATUS",

                "GET_USER_DETAIL",

                "VIEW_DASHBOARD_OVERVIEW"
            };

            foreach (var permission in permissions
                .Where(x => staffPermissions.Contains(x.Name)))
            {
                AddIfNotExists(staff.Id, permission.Id);
            }

            // =========================
            // CUSTOMER
            // Mua hàng
            // =========================
            string[] customerPermissions =
            {
                "CREATE_ORDER",

                "GET_MY_ORDERS",
                "GET_MY_ORDER_DETAIL",

                "CANCEL_ORDER",

                "UPDATE_USER_PROFILE"
            };

            foreach (var permission in permissions
                .Where(x => customerPermissions.Contains(x.Name)))
            {
                AddIfNotExists(customer.Id, permission.Id);
            }

            await context.RolePermissions.AddRangeAsync(toAdd);
            await context.SaveChangesAsync();
        }
    }
}