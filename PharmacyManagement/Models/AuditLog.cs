using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyManagement.Models
{
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EntityName { get; set; }

        [MaxLength(50)]
        public string? EntityId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; }

        [Required]
        public string PerformedById { get; set; }

        [Required]
        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string Details { get; set; }
    }
}
