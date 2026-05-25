using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyManagement.Models
{
    public class Refund
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RefundId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [Required]
        public string RequestedById { get; set; }

        [Required]
        public string Reason { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending / Approved / Rejected / Processed

        [Required]
        [MaxLength(50)]
        public string RefundType { get; set; } // Cancellation / Return

        [Required]
        [MaxLength(50)]
        public string RefundMethod { get; set; } // Card / Cash / BankTransfer

        [Required]
        public decimal Amount { get; set; }

        [MaxLength(100)]
        public string? StripeRefundId { get; set; }

        [MaxLength(50)]
        public string? StripeRefundStatus { get; set; } // succeeded / pending / failed

        public string? AdminNotes { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }
    }
}
