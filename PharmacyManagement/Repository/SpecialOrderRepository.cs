using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using PharmacyManagement.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmacyManagement.Repository
{
    public class SpecialOrderRepository : ISpecialOrderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SpecialOrderRepository> _logger;

        public SpecialOrderRepository(ApplicationDbContext context, ILogger<SpecialOrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SpecialOrder?> GetByIdAsync(int requestId)
        {
            return await _context.SpecialOrders.AsNoTracking().FirstOrDefaultAsync(o => o.RequestId == requestId);
        }

        public async Task<IEnumerable<SpecialOrder>> GetAllAsync()
        {
            return await _context.SpecialOrders.AsNoTracking().OrderByDescending(o => o.DateRequested).ToListAsync();
        }

        public async Task<IEnumerable<SpecialOrder>> GetByDoctorIdAsync(string doctorId)
        {
            return await _context.SpecialOrders
                .Where(o => o.DoctorId == doctorId)
                .AsNoTracking()
                .OrderByDescending(o => o.DateRequested)
                .ToListAsync();
        }

        public async Task<SpecialOrder> CreateAsync(SpecialOrder specialOrder)
        {
            await _context.SpecialOrders.AddAsync(specialOrder);
            await _context.SaveChangesAsync();
            return specialOrder;
        }

        public async Task<bool> UpdateStatusAsync(int requestId, string status, string? adminNotes = null)
        {
            var order = await _context.SpecialOrders.FindAsync(requestId);
            if (order == null) return false;

            order.Status = status;
            order.AdminNotes = adminNotes;
            order.DateResolved = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}