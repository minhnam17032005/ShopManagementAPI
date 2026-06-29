using ShopManagementAPI.Data;
using ShopManagementAPI.Models;
using ShopManagementAPI.Models.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ShopManagementAPI.Repositories
{
    public class OrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        // filter, search, sort, paging
        public IQueryable<Order> Query()
        {
            return _context.Orders
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .AsNoTracking();
        }

        //dành cho management 
        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        //dành cho customer 
        public async Task<Order?> GetByIdAndUserIdAsync(int orderId,int userId)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(x =>
                    x.Id == orderId &&
                    x.UserId == userId);
        }
        public async Task<bool> AnyPendingByUserIdAsync(int userId)
        {
            return await _context.Orders
                .AnyAsync(x =>
                    x.UserId == userId &&
                    x.Status == OrderStatus.PENDING);
        }
        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Orders.CountAsync();
        }

        // đếm số đơn hàng theo trạng thái
        public async Task<int> CountByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .CountAsync(x => x.Status == status);
        }

        // tính tổng doanh thu từ đơn hàng hoàn thành
        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(x => x.Status == OrderStatus.COMPLETED)
                .Select(x => (decimal?)x.TotalAmount)
                .SumAsync() ?? 0;
        }
    }
}
