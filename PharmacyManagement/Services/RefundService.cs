using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PharmacyManagement.Data;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmacyManagement.Services
{
    public class RefundService : IRefundService
    {
        private readonly IRefundRepository _refundRepository;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IDrugsService _drugsService;
        private readonly IAuditService _auditService;
        private readonly ILogger<RefundService> _logger;

        public RefundService(
            IRefundRepository refundRepository,
            ApplicationDbContext context,
            IEmailService emailService,
            IDrugsService drugsService,
            IAuditService auditService,
            ILogger<RefundService> logger,
            IConfiguration configuration)
        {
            _refundRepository = refundRepository;
            _context = context;
            _emailService = emailService;
            _drugsService = drugsService;
            _auditService = auditService;
            _logger = logger;
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        }

        public async Task<RefundDto> RequestRefundAsync(RequestRefundDto dto, string userId)
        {
            var order = await _context.Orders
                .Include(o => o.Drug)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId)
                ?? throw new KeyNotFoundException($"Order {dto.OrderId} not found.");

            if (order.PlacedById != userId)
                throw new UnauthorizedAccessException("You can only request a refund for your own orders.");

            if (dto.RefundType == "Cancellation" && order.Status != "Pending" && order.Status != "Processing")
                throw new InvalidOperationException("Cancellation refund can only be requested for Pending or Processing orders.");

            if (dto.RefundType == "Return" && order.Status != "Delivered")
                throw new InvalidOperationException("Return refund can only be requested for Delivered orders.");

            var existingRefund = await _refundRepository.GetByOrderIdAsync(dto.OrderId);
            if (existingRefund != null)
                throw new InvalidOperationException("A refund request already exists for this order.");

            var refund = new PharmacyManagement.Models.Refund
            {
                OrderId = dto.OrderId,
                RequestedById = userId,
                Reason = dto.Reason,
                RefundType = dto.RefundType,
                RefundMethod = dto.RefundMethod,
                Amount = order.Drug.PricePerUnit * order.Quantity,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            var created = await _refundRepository.CreateAsync(refund);

            var requester = await _context.Users.FindAsync(userId);
            var adminEmail = await _context.Users
                .Where(u => u.Role == "Admin")
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

            if (adminEmail != null)
                await _emailService.SendRefundRequestedAsync(
                    adminEmail,
                    requester?.UserName ?? "Unknown",
                    dto.OrderId,
                    dto.Reason,
                    dto.RefundType);

            _logger.LogInformation("Refund request created for Order {OrderId} by User {UserId}", dto.OrderId, userId);
            await _auditService.LogAsync("Refund", created.RefundId.ToString(), "Created", userId,
                $"Refund requested for Order #{dto.OrderId}, type: {dto.RefundType}, reason: {dto.Reason}.");
            return MapToDto(created);
        }

        public async Task<RefundDto> ProcessRefundAsync(int refundId, ProcessRefundDto dto)
        {
            var refund = await _context.Refunds
                .Include(r => r.Order)
                    .ThenInclude(o => o.Drug)
                .FirstOrDefaultAsync(r => r.RefundId == refundId)
                ?? throw new KeyNotFoundException($"Refund {refundId} not found.");

            if (refund.Status != "Pending")
                throw new InvalidOperationException("Only pending refunds can be processed.");

            if (!dto.IsApproved)
            {
                refund.Status = "Rejected";
                refund.AdminNotes = dto.AdminNotes;
                refund.ResolvedAt = DateTime.UtcNow;
                await _refundRepository.UpdateAsync(refund);

                var requester = await _context.Users.FindAsync(refund.RequestedById);
                if (requester != null)
                    await _emailService.SendRefundProcessedAsync(
                        requester.Email!, requester.UserName!, refund.OrderId,
                        false, refund.Amount, refund.RefundMethod, dto.AdminNotes);

                return MapToDto(refund);
            }

            // Process approved refund
            if (refund.Order.PaymentMethod == "Online")
            {
                var payment = await _context.PaymentTransactions
                    .FirstOrDefaultAsync(p => p.OrderId == refund.OrderId)
                    ?? throw new InvalidOperationException("No payment transaction found for this order.");

                var options = new RefundCreateOptions
                {
                    PaymentIntent = payment.PaymentIntentId,
                    Amount = (long)(refund.Amount * 100)
                };

                var stripeService = new Stripe.RefundService();
                var stripeRefund = await stripeService.CreateAsync(options);

                refund.StripeRefundId = stripeRefund.Id;
                refund.StripeRefundStatus = stripeRefund.Status;
            }

            // Restore stock
            await RestoreStockAsync(refund.Order.DrugId, refund.Order.Quantity);
            await _drugsService.UpdateDrugStockAsync(refund.Order.DrugId);

            // Update order status
            var order = await _context.Orders.FindAsync(refund.OrderId);
            if (order != null)
            {
                order.Status = refund.RefundType == "Cancellation" ? "Cancelled" : "Returned";
                _context.Orders.Update(order);
            }

            refund.Status = "Processed";
            refund.AdminNotes = dto.AdminNotes;
            refund.ResolvedAt = DateTime.UtcNow;
            await _refundRepository.UpdateAsync(refund);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(refund.RequestedById);
            if (user != null)
                await _emailService.SendRefundProcessedAsync(
                    user.Email!, user.UserName!, refund.OrderId,
                    true, refund.Amount, refund.RefundMethod, dto.AdminNotes);

            _logger.LogInformation("Refund {RefundId} processed for Order {OrderId}", refundId, refund.OrderId);
            await _auditService.LogAsync("Refund", refundId.ToString(), dto.IsApproved ? "Approved" : "Rejected", "admin",
                $"Refund #{refundId} for Order #{refund.OrderId} {(dto.IsApproved ? "approved and processed" : "rejected")}.");
            return MapToDto(refund);
        }

        public async Task<IEnumerable<RefundDto>> GetAllRefundsAsync()
        {
            var refunds = await _refundRepository.GetAllAsync();
            return refunds.Select(MapToDto);
        }

        public async Task<RefundDto?> GetRefundByIdAsync(int refundId)
        {
            var refund = await _refundRepository.GetByIdAsync(refundId);
            return refund == null ? null : MapToDto(refund);
        }

        public async Task<IEnumerable<RefundDto>> GetRefundsByUserIdAsync(string userId)
        {
            var refunds = await _refundRepository.GetByUserIdAsync(userId);
            return refunds.Select(MapToDto);
        }

        private async Task RestoreStockAsync(int drugId, int quantity)
        {
            var inventory = await _context.Inventory
                .Where(i => i.DrugId == drugId)
                .OrderByDescending(i => i.LastRestockDate)
                .FirstOrDefaultAsync();

            if (inventory != null)
            {
                inventory.Quantity += quantity;
                _context.Inventory.Update(inventory);
            }
            else
            {
                _context.Inventory.Add(new Inventory
                {
                    DrugId = drugId,
                    Quantity = quantity,
                    LastRestockDate = DateTime.UtcNow,
                    SupplierId = 1
                });
            }

            await _context.SaveChangesAsync();
        }

        private static RefundDto MapToDto(PharmacyManagement.Models.Refund r) => new()
        {
            RefundId = r.RefundId,
            OrderId = r.OrderId,
            RequestedById = r.RequestedById,
            Reason = r.Reason,
            Status = r.Status,
            RefundType = r.RefundType,
            RefundMethod = r.RefundMethod,
            Amount = r.Amount,
            StripeRefundId = r.StripeRefundId,
            StripeRefundStatus = r.StripeRefundStatus,
            AdminNotes = r.AdminNotes,
            RequestedAt = r.RequestedAt,
            ResolvedAt = r.ResolvedAt
        };
    }
}
