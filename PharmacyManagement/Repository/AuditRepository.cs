using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Data;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmacyManagement.Repository
{
    public class AuditRepository : IAuditRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(AuditLog log)
        {
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync(
            string? entityName = null,
            string? action = null,
            string? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(entityName))
                query = query.Where(a => a.EntityName == entityName);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action == action);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(a => a.PerformedById == userId);

            if (startDate.HasValue)
                query = query.Where(a => a.PerformedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.PerformedAt <= endDate.Value);

            return await query
                .OrderByDescending(a => a.PerformedAt)
                .ToListAsync();
        }
    }
}
