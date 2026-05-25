using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Data;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Repository
{
    public class RefundRepository : IRefundRepository
    {
        private readonly ApplicationDbContext _context;

        public RefundRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Refund> CreateAsync(Refund refund)
        {
            _context.Refunds.Add(refund);
            await _context.SaveChangesAsync();
            return refund;
        }

        public async Task<Refund?> GetByIdAsync(int refundId)
        {
            return await _context.Refunds
                .Include(r => r.Order)
                    .ThenInclude(o => o.Drug)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RefundId == refundId);
        }

        public async Task<Refund?> GetByOrderIdAsync(int orderId)
        {
            return await _context.Refunds
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.OrderId == orderId);
        }

        public async Task<IEnumerable<Refund>> GetAllAsync()
        {
            return await _context.Refunds
                .Include(r => r.Order)
                    .ThenInclude(o => o.Drug)
                .AsNoTracking()
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Refund>> GetByUserIdAsync(string userId)
        {
            return await _context.Refunds
                .Include(r => r.Order)
                    .ThenInclude(o => o.Drug)
                .Where(r => r.RequestedById == userId)
                .AsNoTracking()
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<Refund?> UpdateAsync(Refund refund)
        {
            var existing = await _context.Refunds.FindAsync(refund.RefundId);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(refund);
            await _context.SaveChangesAsync();
            return existing;
        }
    }
}
