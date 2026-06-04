using Microsoft.EntityFrameworkCore;
using ShopManagementAPI.Models;
using ShopManagementAPI.Models.Enum;

namespace ShopManagementAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet cho tất cả entity
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Enum
            // RoleType lưu string
            modelBuilder.Entity<Role>()
                .Property(r => r.Name)
                .HasConversion<string>();

            // Permission.Method lưu string
            modelBuilder.Entity<Permission>()
                .Property(p => p.Method)
                .HasConversion<string>();

            // Order.Status lưu string
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();


            //Relationships
            // =========================
            // 1. Role - User (N - N UserRole)
            // =========================
            modelBuilder.Entity<UserRole>()
                 .HasIndex(rp => new { rp.UserId, rp.RoleId })
                 .IsUnique();
            
            modelBuilder.Entity<UserRole>()
                .HasOne(rp => rp.User)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(rp => rp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(rp => rp.Role)  
                .WithMany(p => p.UserRoles)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // 2. Role - Permission (N - N  RolePermission)
            // =========================
            //Set Primary key cho RolePermission là cả roleid và permissionid 
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // unique constraint cho ApiPath + Method + Module
            modelBuilder.Entity<Permission>()
                .HasIndex(p => new { p.ApiPath, p.Method, p.Module })
                .IsUnique();

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // 3. Category - Product (1 - N)
            // =========================
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // 4. User - Order (1 - N)
            // =========================
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // 5. Order - Product (N - N OrderItem)
            // =========================
            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => new { oi.OrderId, oi.ProductId })
                .IsUnique(); // không cho trùng Product trong cùng Order

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}