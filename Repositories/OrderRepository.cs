using Demo_Course_Management.Data;
using Demo_Course_Management.Models;
using Demo_Course_Management.Models.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Demo_Course_Management.Repositories
{
    public class OrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);
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
    }
}
