using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using PharmacyManagement.Data;
using Stripe;

namespace PharmacyManagement.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ApplicationDbContext _context;
        private readonly IDrugsService _drugsService;
        private readonly IEmailService _emailService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IInvoiceService _invoiceService;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            ApplicationDbContext context,
            IDrugsService drugsService,
            IEmailService emailService,
            IPaymentRepository paymentRepository,
            IInvoiceService invoiceService,
            IMapper mapper,
            ILogger<OrderService> logger,
            IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _context = context;
            _drugsService = drugsService;
            _emailService = emailService;
            _paymentRepository = paymentRepository;
            _invoiceService = invoiceService;
            _mapper = mapper;
            _logger = logger;
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
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

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(string userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string userId)
        {
            _logger.LogInformation("Creating order for User {UserId} and Drug {DrugId}", userId, dto.DrugId);

            var drug = await _context.Drugs.FindAsync(dto.DrugId)
                       ?? throw new KeyNotFoundException("Drug not found.");

            if (drug.Stock < dto.Quantity)
                throw new InvalidOperationException("Insufficient stock available.");

            var hasValidBatch = await _context.Inventory
                .AnyAsync(i => i.DrugId == dto.DrugId
                    && i.Quantity > 0
                    && (!i.ExpiryDate.HasValue || i.ExpiryDate.Value.Date > DateTime.UtcNow.Date));

            if (!hasValidBatch)
                throw new InvalidOperationException("Cannot place order: all available batches for this drug are expired.");

            var order = _mapper.Map<Order>(dto);
            order.PlacedById = userId;
            order.PaymentMethod = dto.PaymentMethod;
            var createdOrder = await _orderRepository.CreateOrderAsync(order);

            var result = _mapper.Map<OrderDto>(createdOrder);
            result.ClientSecret = await HandlePaymentOnOrderCreation(createdOrder, drug, dto.PaymentMethod);

            var placer = await _context.Users.FindAsync(userId);
            await _emailService.SendNewOrderNotificationAsync(
                placer?.UserName ?? "Unknown",
                createdOrder.Id,
                drug.Name,
                dto.Quantity
            );

            return result;
        }

        public async Task<OrderDto> CreatePatientOrderAsync(CreatePatientOrderDto dto, string userId)
        {
            _logger.LogInformation("Creating patient order for User {UserId} and Drug {DrugId}", userId, dto.DrugId);

            var drug = await _context.Drugs.FindAsync(dto.DrugId)
                       ?? throw new KeyNotFoundException("Drug not found.");

            if (drug.IsPrescriptionRequired && string.IsNullOrWhiteSpace(dto.PrescriptionReference))
                throw new InvalidOperationException("Prescription reference is required for this drug.");

            if (drug.Stock < dto.Quantity)
                throw new InvalidOperationException("Insufficient stock available.");

            var hasValidBatch = await _context.Inventory
                .AnyAsync(i => i.DrugId == dto.DrugId
                    && i.Quantity > 0
                    && (!i.ExpiryDate.HasValue || i.ExpiryDate.Value.Date > DateTime.UtcNow.Date));

            if (!hasValidBatch)
                throw new InvalidOperationException("Cannot place order: all available batches for this drug are expired.");

            var order = new Order
            {
                DrugId = dto.DrugId,
                Quantity = dto.Quantity,
                PrescriptionReference = dto.PrescriptionReference,
                PlacedById = userId,
                Status = "Pending",
                PlacedAt = DateTime.Now,
                PaymentMethod = dto.PaymentMethod
            };

            var createdOrder = await _orderRepository.CreateOrderAsync(order);

            var result = _mapper.Map<OrderDto>(createdOrder);
            result.ClientSecret = await HandlePaymentOnOrderCreation(createdOrder, drug, dto.PaymentMethod);

            var placer = await _context.Users.FindAsync(userId);
            await _emailService.SendNewOrderNotificationAsync(
                placer?.UserName ?? "Unknown",
                createdOrder.Id,
                drug.Name,
                dto.Quantity
            );

            return result;
        }

        public async Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderDto dto)
        {
            var existing = await _orderRepository.GetOrderByIdAsync(id)
                           ?? throw new KeyNotFoundException($"Order with ID {id} not found.");

            var oldStatus = existing.Status;
            _mapper.Map(dto, existing);

            if (oldStatus != "Delivered" && dto.Status == "Delivered")
            {
                await DeductInventoryAsync(existing.DrugId, existing.Quantity);
                await _drugsService.UpdateDrugStockAsync(existing.DrugId);
                existing.DateDispensed = DateTime.Now;

                await CreateSaleFromOrderAsync(existing);

                // Update cash payment transaction to Paid on delivery
                if (existing.PaymentMethod == "Cash")
                    await _paymentRepository.UpdateStatusAsync(
                        $"CASH-{existing.Id}", "Paid");

                var placer = await _context.Users.FindAsync(existing.PlacedById);
                if (placer != null)
                {
                    await _emailService.SendOrderStatusEmailAsync(
                        placer.Email!, placer.UserName!, existing.Id, existing.Drug?.Name ?? "Drug", "Delivered");

                    // Generate and email invoice
                    var invoicePdf = await _invoiceService.GenerateInvoiceAsync(existing.Id);
                    await _emailService.SendInvoiceEmailAsync(placer.Email!, placer.UserName!, existing.Id, invoicePdf);
                }
            }
            else if (oldStatus == "Delivered" && dto.Status != "Delivered")
            {
                await RestoreInventoryAsync(existing.DrugId, existing.Quantity);
                await _drugsService.UpdateDrugStockAsync(existing.DrugId);
                existing.DateDispensed = null;
            }
            else if (dto.Status == "Cancelled")
            {
                var placer = await _context.Users.FindAsync(existing.PlacedById);
                if (placer != null)
                    await _emailService.SendOrderStatusEmailAsync(
                        placer.Email!, placer.UserName!, existing.Id, existing.Drug?.Name ?? "Drug", "Cancelled");
            }

            var updated = await _orderRepository.UpdateOrderAsync(existing);
            return _mapper.Map<OrderDto?>(updated);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id)
                       ?? throw new KeyNotFoundException($"Order with ID {id} not found.");

            if (order.Status == "Delivered")
            {
                await RestoreInventoryAsync(order.DrugId, order.Quantity);
                await _drugsService.UpdateDrugStockAsync(order.DrugId);
            }

            return await _orderRepository.DeleteOrderAsync(id);
        }

        // Returns ClientSecret for Online payments, null for Cash
        private async Task<string?> HandlePaymentOnOrderCreation(Order order, Drug drug, string paymentMethod)
        {
            if (paymentMethod == "Online")
            {
                var amountInCents = (long)(drug.PricePerUnit * order.Quantity * 100);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = "usd",
                    Metadata = new Dictionary<string, string>
                    {
                        { "OrderId", order.Id.ToString() },
                        { "DrugName", drug.Name }
                    }
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                await _paymentRepository.CreateAsync(new PaymentTransaction
                {
                    PaymentIntentId = intent.Id,
                    Status = "Pending",
                    Amount = drug.PricePerUnit * order.Quantity,
                    Currency = "usd",
                    PaymentMethod = "Online",
                    OrderId = order.Id
                });

                return intent.ClientSecret;
            }
            else
            {
                // Cash — store a pending transaction with a placeholder ID
                await _paymentRepository.CreateAsync(new PaymentTransaction
                {
                    PaymentIntentId = $"CASH-{order.Id}",
                    Status = "Pending",
                    Amount = drug.PricePerUnit * order.Quantity,
                    Currency = "usd",
                    PaymentMethod = "Cash",
                    OrderId = order.Id
                });

                return null;
            }
        }

        private async Task CreateSaleFromOrderAsync(Order order)
        {
            var existingSale = await _context.Sales.FirstOrDefaultAsync(s => s.OrderId == order.Id);
            if (existingSale != null) return;

            var drug = order.Drug ?? await _context.Drugs.FindAsync(order.DrugId);
            if (drug == null) return;

            var sale = new Sales
            {
                Date = order.DateDispensed ?? DateTime.UtcNow,
                TotalAmount = drug.PricePerUnit * order.Quantity,
                Quantity = order.Quantity,
                UnitPrice = drug.PricePerUnit,
                DrugId = order.DrugId,
                OrderId = order.Id,
                PaymentMethod = order.PaymentMethod
            };

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
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
                _context.Inventory.Add(new Inventory
                {
                    DrugId = drugId,
                    Quantity = quantityToRestore,
                    LastRestockDate = DateTime.Now,
                    SupplierId = 1
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
