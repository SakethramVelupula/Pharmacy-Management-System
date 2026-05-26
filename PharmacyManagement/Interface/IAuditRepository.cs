using PharmacyManagement.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface IAuditRepository
    {
        Task CreateAsync(AuditLog log);
        Task<IEnumerable<AuditLog>> GetAllAsync(
            string? entityName = null,
            string? action = null,
            string? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
    }
}
