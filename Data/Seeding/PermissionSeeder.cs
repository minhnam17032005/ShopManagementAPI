using Demo_Course_Management.Models.Enum;
using Demo_Course_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_Course_Management.Data.Seeding
{
    public static class PermissionSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            //lấy permission đã có (key unique)
            var existing = await context.Permissions
                .Select(p => new { p.ApiPath, p.Method, p.Module })
                .ToListAsync();

            var permissions = new List<Permission>
        {
             // Category
            new Permission { Name = "CREATE_CATEGORY", Description = "Tạo danh mục", ApiPath = "/api/categories", Method = HttpMethodType.POST, Module = "Category" },
            new Permission { Name = "GET_CATEGORIES", Description = "Lấy danh sách danh mục", ApiPath = "/api/categories", Method = HttpMethodType.GET, Module = "Category" },
            new Permission { Name = "GET_CATEGORY_BY_ID", Description = "Lấy chi tiết danh mục", ApiPath = "/api/categories/{id}", Method = HttpMethodType.GET, Module = "Category" },
            new Permission { Name = "UPDATE_CATEGORY", Description = "Cập nhật danh mục", ApiPath = "/api/categories/{id}", Method = HttpMethodType.PUT, Module = "Category" },
            new Permission { Name = "DELETE_CATEGORY", Description = "Xóa danh mục", ApiPath = "/api/categories/{id}", Method = HttpMethodType.DELETE, Module = "Category" },

            // Product
            new Permission { Name = "CREATE_PRODUCT", Description = "Tạo sản phẩm", ApiPath = "/api/products", Method = HttpMethodType.POST, Module = "Product" },
            new Permission { Name = "GET_PRODUCTS", Description = "Lấy danh sách sản phẩm", ApiPath = "/api/products", Method = HttpMethodType.GET, Module = "Product" },
            new Permission { Name = "GET_PRODUCT_BY_ID", Description = "Lấy chi tiết sản phẩm", ApiPath = "/api/products/{id}", Method = HttpMethodType.GET, Module = "Product" },
            new Permission { Name = "UPDATE_PRODUCT", Description = "Cập nhật sản phẩm", ApiPath = "/api/products/{id}", Method = HttpMethodType.PUT, Module = "Product" },
            new Permission { Name = "DELETE_PRODUCT", Description = "Xóa sản phẩm", ApiPath = "/api/products/{id}", Method = HttpMethodType.DELETE, Module = "Product" },

            // Permission
            new Permission { Name = "GET_PERMISSIONS", Description = "Lấy danh sách quyền", ApiPath = "/api/permissions", Method = HttpMethodType.GET, Module = "Permission" },
            new Permission { Name = "GET_PERMISSION_BY_ID", Description = "Lấy chi tiết quyền", ApiPath = "/api/permissions/{id}", Method = HttpMethodType.GET, Module = "Permission" },

            // Role
            new Permission { Name = "GET_ROLES", Description = "Lấy danh sách vai trò", ApiPath = "/api/roles", Method = HttpMethodType.GET, Module = "Role" },
            new Permission { Name = "GET_ROLE_BY_ID", Description = "Lấy chi tiết vai trò", ApiPath = "/api/roles/{id}", Method = HttpMethodType.GET, Module = "Role" },
            new Permission { Name = "ADD_PERMISSION_TO_ROLE", Description = "Thêm quyền vào vai trò", ApiPath = "/api/roles/{id}/permissions", Method = HttpMethodType.POST, Module = "Role" },
            new Permission { Name = "REMOVE_PERMISSION_FROM_ROLE", Description = "Xóa quyền khỏi vai trò", ApiPath = "/api/roles/{id}/permissions", Method = HttpMethodType.DELETE, Module = "Role" }
        };

            //lọc permission chưa tồn tại
            var toAdd = permissions
                .Where(p => !existing.Any(e =>
                    e.ApiPath == p.ApiPath &&
                    e.Method == p.Method &&
                    e.Module == p.Module))
                .ToList();

            if (toAdd.Any())
            {
                context.Permissions.AddRange(toAdd);
                await context.SaveChangesAsync();
            }
        }
    }
}
