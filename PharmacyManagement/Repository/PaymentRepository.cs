using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Data;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using System;
using System.Threading.Tasks;

namespace PharmacyManagement.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction)
        {
            _context.PaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<PaymentTransaction?> GetByPaymentIntentIdAsync(string paymentIntentId)
        {
            return await _context.PaymentTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntentId);
        }

        public async Task<PaymentTransaction?> UpdateStatusAsync(string paymentIntentId, string status)
        {
            var transaction = await _context.PaymentTransactions
                .FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntentId);

            if (transaction == null) return null;

            transaction.Status = status;
            transaction.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return transaction;
        }
    }
}
