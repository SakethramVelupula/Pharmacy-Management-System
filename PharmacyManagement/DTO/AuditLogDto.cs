using System;

namespace PharmacyManagement.DTO
{
    public class AuditLogDto
    {
        public int AuditId { get; set; }
        public string EntityName { get; set; }
        public string? EntityId { get; set; }
        public string Action { get; set; }
        public string PerformedById { get; set; }
        public string? PerformedByName { get; set; }
        public DateTime PerformedAt { get; set; }
        public string Details { get; set; }
    }
}
