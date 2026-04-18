using Demo_Course_Management.Data;
using Demo_Course_Management.Models;
using Demo_Course_Management.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace Demo_Course_Management.Repositories
{
    public class OrderItemRepository
    {
        private readonly AppDbContext _context;

        public OrderItemRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<bool> ExistsInPendingOrderAsync(int productId)
        {
            return await _context.OrderItems
                .Include(x => x.Order)
                .AnyAsync(x =>
                    x.ProductId == productId &&
                    x.Order.Status == OrderStatus.PENDING);
        }

        public async Task AddAsync(OrderItem item)
        {
            await _context.OrderItems.AddAsync(item);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
