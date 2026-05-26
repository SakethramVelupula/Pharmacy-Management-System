using PharmacyManagement.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface IAuditService
    {
        Task LogAsync(string entityName, string? entityId, string action, string performedById, string details);
        Task<IEnumerable<AuditLogDto>> GetLogsAsync(
            string? entityName = null,
            string? action = null,
            string? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
    }
}
