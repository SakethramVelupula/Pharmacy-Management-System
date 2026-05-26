using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Data;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmacyManagement.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _auditRepository;
        private readonly ApplicationDbContext _context;

        public AuditService(IAuditRepository auditRepository, ApplicationDbContext context)
        {
            _auditRepository = auditRepository;
            _context = context;
        }

        public async Task LogAsync(string entityName, string? entityId, string action, string performedById, string details)
        {
            var log = new AuditLog
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                PerformedById = performedById,
                PerformedAt = DateTime.UtcNow,
                Details = details
            };

            await _auditRepository.CreateAsync(log);
        }

        public async Task<IEnumerable<AuditLogDto>> GetLogsAsync(
            string? entityName = null,
            string? action = null,
            string? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var logs = await _auditRepository.GetAllAsync(entityName, action, userId, startDate, endDate);

            var userIds = logs.Select(l => l.PerformedById).Distinct().ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            return logs.Select(l => new AuditLogDto
            {
                AuditId = l.AuditId,
                EntityName = l.EntityName,
                EntityId = l.EntityId,
                Action = l.Action,
                PerformedById = l.PerformedById,
                PerformedByName = users.FirstOrDefault(u => u.Id == l.PerformedById)?.UserName ?? "Unknown",
                PerformedAt = l.PerformedAt,
                Details = l.Details
            });
        }
    }
}
