using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using PharmacyManagement.Data;

namespace PharmacyManagement.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ApplicationDbContext _context;
        private readonly IDrugsService _drugsService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IOrderRepository orderRepository, ApplicationDbContext context, IDrugsService drugsService, IEmailService emailService, IMapper mapper, ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _context = context;
            _drugsService = drugsService;
            _emailService = emailService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            return _mapper.Map<OrderDto?>(order);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByDoctorIdAsync(string doctorId)
        {
            var orders = await _orderRepository.GetOrdersByDoctorIdAsync(doctorId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string userId)
        {
            _logger.LogInformation("Creating order for User {UserId} and Drug {DrugId}", userId, dto.DrugId);

            var drug = await _context.Drugs.FindAsync(dto.DrugId)
                       ?? throw new KeyNotFoundException("Drug not found.");

            if (drug.Stock < dto.Quantity)
                throw new InvalidOperationException("Insufficient stock available.");

            var order = _mapper.Map<Order>(dto);
            order.DoctorId = userId; // Set DoctorId from authenticated user
            var createdOrder = await _orderRepository.CreateOrderAsync(order);

            // Notify admin about new order
            var doctor = await _context.Users.FindAsync(userId);
            await _emailService.SendNewOrderNotificationAsync(
                doctor?.UserName ?? "Unknown",
                createdOrder.Id,
                drug.Name,
                dto.Quantity
            );

            return _mapper.Map<OrderDto>(createdOrder);
        }

        public async Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderDto dto)
        {
            var existing = await _orderRepository.GetOrderByIdAsync(id)
                           ?? throw new KeyNotFoundException($"Order with ID {id} not found.");

            var oldStatus = existing.Status;
            _mapper.Map(dto, existing);

            // If status changed to "Delivered", deduct inventory and create sale
            if (oldStatus != "Delivered" && dto.Status == "Delivered")
            {
                await DeductInventoryAsync(existing.DrugId, existing.Quantity);
                await _drugsService.UpdateDrugStockAsync(existing.DrugId);
                existing.DateDispensed = DateTime.Now;
                await CreateSaleFromOrderAsync(existing.Id);

                // Notify doctor order is delivered
                var doctor = await _context.Users.FindAsync(existing.DoctorId);
                if (doctor != null)
                    await _emailService.SendOrderStatusEmailAsync(
                        doctor.Email!, doctor.UserName!, existing.Id, existing.Drug?.Name ?? "Drug", "Delivered"
                    );
            }
            // If status changed from "Delivered" to something else, restore inventory
            else if (oldStatus == "Delivered" && dto.Status != "Delivered")
            {
                await RestoreInventoryAsync(existing.DrugId, existing.Quantity);
                await _drugsService.UpdateDrugStockAsync(existing.DrugId);
                existing.DateDispensed = null;
            }
            else if (dto.Status == "Cancelled")
            {
                // Notify doctor order is cancelled
                var doctor = await _context.Users.FindAsync(existing.DoctorId);
                if (doctor != null)
                    await _emailService.SendOrderStatusEmailAsync(
                        doctor.Email!, doctor.UserName!, existing.Id, existing.Drug?.Name ?? "Drug", "Cancelled"
                    );
            }

            var updated = await _orderRepository.UpdateOrderAsync(existing);
            return _mapper.Map<OrderDto?>(updated);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id)
                       ?? throw new KeyNotFoundException($"Order with ID {id} not found.");

            // If order was delivered, restore inventory before deletion
            if (order.Status == "Delivered")
            {
                await RestoreInventoryAsync(order.DrugId, order.Quantity);
                await _drugsService.UpdateDrugStockAsync(order.DrugId);
            }

            var result = await _orderRepository.DeleteOrderAsync(id);
            return result;
        }
        private async Task DeductInventoryAsync(int drugId, int quantityNeeded)
        {
            var inventories = await _context.Inventory
                .Where(i => i.DrugId == drugId && i.Quantity > 0)
                .OrderBy(i => i.LastRestockDate)
                .ToListAsync();

            foreach (var inventory in inventories)
            {
                if (quantityNeeded <= 0) break;

                if (inventory.Quantity >= quantityNeeded)
                {
                    inventory.Quantity -= quantityNeeded;
                    quantityNeeded = 0;
                }
                else
                {
                    quantityNeeded -= inventory.Quantity;
                    inventory.Quantity = 0;
                }
                _context.Inventory.Update(inventory);
            }

            if (quantityNeeded > 0)
                throw new InvalidOperationException("Not enough inventory batches to fulfill this order.");

            await _context.SaveChangesAsync();
        }

        private async Task RestoreInventoryAsync(int drugId, int quantityToRestore)
        {
            var latestInventory = await _context.Inventory
                .Where(i => i.DrugId == drugId)
                .OrderByDescending(i => i.LastRestockDate)
                .FirstOrDefaultAsync();

            if (latestInventory != null)
            {
                latestInventory.Quantity += quantityToRestore;
                _context.Inventory.Update(latestInventory);
            }
            else
            {
                var newInventory = new Inventory
                {
                    DrugId = drugId,
                    Quantity = quantityToRestore,
                    LastRestockDate = DateTime.Now,
                    SupplierId = 1 // Default supplier - adjust as needed
                };
                _context.Inventory.Add(newInventory);
            }

            await _context.SaveChangesAsync();
        }

        private async Task CreateSaleFromOrderAsync(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Drug)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null) return;

                // Check if sale already exists
                var existingSale = await _context.Sales.FirstOrDefaultAsync(s => s.OrderId == orderId);
                if (existingSale != null) return;

                var sale = new Sales
                {
                    Date = order.DateDispensed ?? DateTime.UtcNow,
                    TotalAmount = order.Drug.Price * order.Quantity,
                    Quantity = order.Quantity,
                    UnitPrice = order.Drug.Price,
                    DrugId = order.DrugId,
                    OrderId = order.Id,
                    PaymentMethod = "Cash"
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Auto-created sale record for Order {OrderId} - Amount: {Amount}", orderId, sale.TotalAmount);
                Console.WriteLine($"SALE CREATED: OrderId={orderId}, Amount={sale.TotalAmount}, Drug={order.Drug.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create sale record for Order {OrderId}", orderId);
            }
        }
    }
}