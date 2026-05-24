using AutoMapper;
using Microsoft.Extensions.Logging;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace PharmacyManagement.Services
{
    public class SalesService : ISalesService
    {
        private readonly ISalesRepository _salesRepository;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SalesService> _logger;

        public SalesService(ISalesRepository repo, ApplicationDbContext context, IMapper mapper, ILogger<SalesService> logger)
        {
            _salesRepository = repo;
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<SaleDto>> GetAllSalesAsync()
        {
            var sales = await _context.Sales
                .Include(s => s.Drug)
                .Include(s => s.Order)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            return sales.Select(s => new SaleDto
            {
                SalesId = s.SalesId,
                Date = s.Date,
                TotalAmount = s.TotalAmount,
                Quantity = s.Quantity,
                UnitPrice = s.UnitPrice,
                DrugId = s.DrugId,
                DrugName = s.Drug?.Name ?? "Unknown",
                OrderId = s.OrderId.GetValueOrDefault(),
                PaymentMethod = s.PaymentMethod
            });
        }

        public async Task<SaleDto> GetSaleByIdAsync(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Drug)
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.SalesId == id);

            if (sale == null)
                throw new KeyNotFoundException($"Sale with ID {id} not found.");

            return new SaleDto
            {
                SalesId = sale.SalesId,
                Date = sale.Date,
                TotalAmount = sale.TotalAmount,
                Quantity = sale.Quantity,
                UnitPrice = sale.UnitPrice,
                DrugId = sale.DrugId,
                DrugName = sale.Drug.Name,
                OrderId = sale.OrderId.GetValueOrDefault(),
                PaymentMethod = sale.PaymentMethod
            };
        }

        public async Task<SaleDto> CreateSaleFromOrderAsync(CreateSaleDto dto)
        {
            var order = await _context.Orders
                .Include(o => o.Drug)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
                throw new KeyNotFoundException($"Order with ID {dto.OrderId} not found.");

            if (order.Status != "Dispensed")
                throw new InvalidOperationException("Can only create sales from dispensed orders.");

            var existingSale = await _context.Sales.FirstOrDefaultAsync(s => s.OrderId == dto.OrderId);
            if (existingSale != null)
                throw new InvalidOperationException("Sale already exists for this order.");

            var sale = new Sales
            {
                Date = order.DateDispensed ?? DateTime.UtcNow,
                TotalAmount = order.Drug.PricePerUnit * order.Quantity,
                Quantity = order.Quantity,
                UnitPrice = order.Drug.PricePerUnit,
                DrugId = order.DrugId,
                OrderId = order.Id,
                PaymentMethod = dto.PaymentMethod
            };

            var created = await _salesRepository.CreateAsync(sale);

            return new SaleDto
            {
                SalesId = created.SalesId,
                Date = created.Date,
                TotalAmount = created.TotalAmount,
                Quantity = created.Quantity,
                UnitPrice = created.UnitPrice,
                DrugId = created.DrugId,
                DrugName = order.Drug.Name,
                OrderId = created.OrderId.GetValueOrDefault(),
                PaymentMethod = created.PaymentMethod
            };
        }

        public async Task<SalesAnalyticsDto> GetSalesAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var sales = await _context.Sales
                .Include(s => s.Drug)
                .Where(s => s.Date >= startDate && s.Date <= endDate)
                .ToListAsync();

            var totalRevenue = sales.Sum(s => s.TotalAmount);
            var totalSales = sales.Count;
            var averageOrderValue = totalSales > 0 ? totalRevenue / totalSales : 0;

            var topSellingDrugs = sales
                .Where(s => s.Drug != null)
                .GroupBy(s => new { s.DrugId, DrugName = s.Drug.Name })
                .Select(g => new TopSellingDrugDto
                {
                    DrugName = g.Key.DrugName ?? "Unknown",
                    TotalQuantity = g.Sum(s => s.Quantity),
                    TotalRevenue = g.Sum(s => s.TotalAmount)
                })
                .OrderByDescending(d => d.TotalRevenue)
                .Take(10)
                .ToList();

            var dailySales = sales
                .GroupBy(s => s.Date.Date)
                .Select(g => new DailySalesDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(s => s.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            var paymentMethods = sales
                .GroupBy(s => s.PaymentMethod)
                .Select(g => new PaymentMethodDto
                {
                    Method = g.Key ?? "Unknown",
                    Count = g.Count(),
                    Amount = g.Sum(s => s.TotalAmount)
                })
                .ToList();

            return new SalesAnalyticsDto
            {
                TotalRevenue = totalRevenue,
                TotalSales = totalSales,
                AverageOrderValue = averageOrderValue,
                TopSellingDrugs = topSellingDrugs ?? new List<TopSellingDrugDto>(),
                DailySales = dailySales ?? new List<DailySalesDto>(),
                PaymentMethods = paymentMethods ?? new List<PaymentMethodDto>()
            };
        }
    }
}
