using ShopManagementAPI.Models.Enum;
using ShopManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopManagementAPI.Data.Seeding
{
    public static class PermissionSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var permissions = new List<Permission>
            {
                // CATEGORY
                new() { Name = "CREATE_CATEGORY", Description = "Tạo danh mục", ApiPath = "/api/categories", Method = HttpMethodType.POST, Module = "CATEGORY" },
                new() { Name = "UPDATE_CATEGORY", Description = "Cập nhật danh mục", ApiPath = "/api/categories/{id}", Method = HttpMethodType.PUT, Module = "CATEGORY" },
                new() { Name = "DELETE_CATEGORY", Description = "Xóa danh mục", ApiPath = "/api/categories/{id}", Method = HttpMethodType.DELETE, Module = "CATEGORY" },
                new() { Name = "RESTORE_CATEGORY", Description = "Khôi phục danh mục", ApiPath = "/api/categories/{id}/restore", Method = HttpMethodType.PATCH, Module = "CATEGORY" },

                // PRODUCT
                new() { Name = "CREATE_PRODUCT", Description = "Tạo sản phẩm", ApiPath = "/api/products", Method = HttpMethodType.POST, Module = "PRODUCT" },
                new() { Name = "UPDATE_PRODUCT", Description = "Cập nhật sản phẩm", ApiPath = "/api/products/{id}", Method = HttpMethodType.PUT, Module = "PRODUCT" },
                new() { Name = "DELETE_PRODUCT", Description = "Xóa sản phẩm", ApiPath = "/api/products/{id}", Method = HttpMethodType.DELETE, Module = "PRODUCT" },
                new() { Name = "UPDATE_PRODUCT_STOCK", Description = "Cập nhật tồn kho", ApiPath = "/api/products/{id}/stock", Method = HttpMethodType.PATCH, Module = "PRODUCT" },
                new() { Name = "RESTORE_PRODUCT", Description = "Khôi phục sản phẩm", ApiPath = "/api/products/{id}/restore", Method = HttpMethodType.PATCH, Module = "PRODUCT" },

                // ORDER
                new() { Name = "CREATE_ORDER", Description = "Tạo đơn hàng", ApiPath = "/api/orders", Method = HttpMethodType.POST, Module = "ORDER" },
                new() { Name = "GET_ORDERS", Description = "Danh sách đơn hàng", ApiPath = "/api/orders", Method = HttpMethodType.GET, Module = "ORDER" },
                new() { Name = "GET_ORDER_DETAIL", Description = "Chi tiết đơn hàng", ApiPath = "/api/orders/{id}", Method = HttpMethodType.GET, Module = "ORDER" },
                new() { Name = "UPDATE_ORDER_STATUS", Description = "Cập nhật trạng thái đơn hàng", ApiPath = "/api/orders/{id}/status", Method = HttpMethodType.PATCH, Module = "ORDER" },
                new() { Name = "GET_MY_ORDERS", Description = "Danh sách đơn hàng của tôi",ApiPath = "/api/orders/my-orders",Method = HttpMethodType.GET,Module = "ORDER"},
                new() { Name = "GET_MY_ORDER_DETAIL", Description = "Chi tiết đơn hàng của tôi", ApiPath = "/api/orders/my-orders/{id}", Method = HttpMethodType.GET, Module = "ORDER"},
                new() { Name = "CANCEL_ORDER", Description = "Hủy đơn hàng", ApiPath = "/api/orders/{id}/cancel", Method = HttpMethodType.PATCH, Module = "ORDER"},
                
                
                // USER
                new() { Name = "CREATE_USER", Description = "Tạo user", ApiPath = "/api/users", Method = HttpMethodType.POST, Module = "USER" },
                new() { Name = "GET_USERS", Description = "Danh sách user", ApiPath = "/api/users", Method = HttpMethodType.GET, Module = "USER" },
                new() { Name = "GET_USER_DETAIL", Description = "Chi tiết user", ApiPath = "/api/users/{id}", Method = HttpMethodType.GET, Module = "USER" },
                new() { Name = "UPDATE_USER_PROFILE", Description = "Cập nhật profile", ApiPath = "/api/users/profile", Method = HttpMethodType.PUT,Module = "USER"},                new() { Name = "ADD_USER_ROLES", Description = "Thêm role cho user", ApiPath = "/api/users/{id}/roles", Method = HttpMethodType.POST, Module = "USER" },
                new() { Name = "REMOVE_USER_ROLES", Description = "Xóa role khỏi user", ApiPath = "/api/users/{id}/roles", Method = HttpMethodType.DELETE, Module = "USER" },
                new() { Name = "LOCK_USER", Description = "Khóa tài khoản", ApiPath = "/api/users/{id}/lock", Method = HttpMethodType.PATCH, Module = "USER" },
                new() { Name = "UNLOCK_USER", Description = "Mở khóa tài khoản", ApiPath = "/api/users/{id}/unlock", Method = HttpMethodType.PATCH, Module = "USER" },

                // ROLE
                new() { Name = "GET_ROLES", Description = "Danh sách role", ApiPath = "/api/roles", Method = HttpMethodType.GET, Module = "ROLE" },
                new() { Name = "GET_ROLE_DETAIL", Description = "Chi tiết role", ApiPath = "/api/roles/{id}", Method = HttpMethodType.GET, Module = "ROLE" },
                new() { Name = "ADD_ROLE_PERMISSIONS", Description = "Gán quyền cho role", ApiPath = "/api/roles/{id}/permissions", Method = HttpMethodType.POST, Module = "ROLE" },
                new() { Name = "REMOVE_ROLE_PERMISSIONS", Description = "Xóa quyền khỏi role", ApiPath = "/api/roles/{id}/permissions", Method = HttpMethodType.DELETE, Module = "ROLE" },

                // PERMISSION
                new() { Name = "GET_PERMISSIONS", Description = "Danh sách permission", ApiPath = "/api/permissions", Method = HttpMethodType.GET, Module = "PERMISSION" },
                new() { Name = "GET_PERMISSION_DETAIL", Description = "Chi tiết permission", ApiPath = "/api/permissions/{id}", Method = HttpMethodType.GET, Module = "PERMISSION" },
            
                //DASHBOARD
                new() { Name = "VIEW_DASHBOARD_OVERVIEW", Description = "Xem dashboard tổng quan",ApiPath = "/api/dashboard/overview",Method = HttpMethodType.GET,Module = "DASHBOARD"},
                new() { Name = "VIEW_REVENUE", Description = "Xem doanh thu", ApiPath = "/api/dashboard/revenue", Method = HttpMethodType.GET, Module = "DASHBOARD"}
            };

            foreach (var p in permissions)
            {
                var exists = await context.Permissions.AnyAsync(x => x.Name == p.Name);
                if (!exists)
                    await context.Permissions.AddAsync(p);
            }

            await context.SaveChangesAsync();
        }
    }
    }

