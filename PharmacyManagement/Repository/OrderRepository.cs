
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using PharmacyManagement.Data;

namespace PharmacyManagement.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(ApplicationDbContext context, ILogger<OrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            _logger.LogInformation("Fetching all orders.");
            return await _context.Orders.Include(o => o.Drug).AsNoTracking().ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            _logger.LogInformation("Fetching order with ID {OrderId}", id);
            return await _context.Orders.Include(o => o.Drug).AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersByDoctorIdAsync(string doctorId)
        {
            _logger.LogInformation("Fetching orders for Doctor ID {DoctorId}", doctorId);
            return await _context.Orders
                .Where(o => o.DoctorId == doctorId)
                .Include(o => o.Drug)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            _logger.LogInformation("Creating new order.");
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order?> UpdateOrderAsync(Order order)
        {
            _logger.LogInformation("Updating order ID {OrderId}", order.Id);
            var existingOrder = await _context.Orders.FindAsync(order.Id);
            if (existingOrder == null)
                return null;

            _context.Entry(existingOrder).CurrentValues.SetValues(order);
            await _context.SaveChangesAsync();
            return existingOrder;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            _logger.LogInformation("Deleting order ID {OrderId}", id);
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}