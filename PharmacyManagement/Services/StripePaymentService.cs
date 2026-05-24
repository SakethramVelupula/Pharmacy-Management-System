using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PharmacyManagement.Data;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Services
{
    public class StripePaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ISalesRepository _salesRepository;
        private readonly ApplicationDbContext _context;

        public StripePaymentService(
            IPaymentRepository paymentRepository,
            ISalesRepository salesRepository,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _paymentRepository = paymentRepository;
            _salesRepository = salesRepository;
            _context = context;
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        }

        public async Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Drug)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new KeyNotFoundException($"Order {orderId} not found.");

            if (order.Status != "Processing" && order.Status != "Pending")
                throw new InvalidOperationException("Payment can only be initiated for Pending or Processing orders.");

            var amountInCents = (long)(order.Drug.PricePerUnit * order.Quantity * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = "usd",
                Metadata = new Dictionary<string, string>
                {
                    { "OrderId", orderId.ToString() },
                    { "DrugName", order.Drug.Name }
                }
            };

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            await _paymentRepository.CreateAsync(new PaymentTransaction
            {
                PaymentIntentId = intent.Id,
                Status = "Pending",
                Amount = order.Drug.PricePerUnit * order.Quantity,
                Currency = "usd",
                OrderId = orderId
            });

            return new PaymentIntentResponseDto
            {
                PaymentIntentId = intent.Id,
                ClientSecret = intent.ClientSecret,
                Amount = order.Drug.PricePerUnit * order.Quantity,
                Currency = "usd",
                Status = intent.Status
            };
        }

        public async Task<SaleDto> ConfirmPaymentAsync(ConfirmPaymentDto dto)
        {
            var service = new PaymentIntentService();
            var intent = await service.GetAsync(dto.PaymentIntentId);

            if (intent.Status != "succeeded")
                throw new InvalidOperationException($"Payment not completed. Current status: {intent.Status}");

            await _paymentRepository.UpdateStatusAsync(dto.PaymentIntentId, "succeeded");

            var order = await _context.Orders
                .Include(o => o.Drug)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId)
                ?? throw new KeyNotFoundException($"Order {dto.OrderId} not found.");

            var existingSale = await _context.Sales.FirstOrDefaultAsync(s => s.OrderId == dto.OrderId);
            if (existingSale != null)
                throw new InvalidOperationException("Sale already exists for this order.");

            var sale = new Sales
            {
                Date = DateTime.UtcNow,
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
    }
}
