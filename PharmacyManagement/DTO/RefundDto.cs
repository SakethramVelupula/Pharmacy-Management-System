using System;
using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.DTO
{
    public class RefundDto
    {
        public int RefundId { get; set; }
        public int OrderId { get; set; }
        public string RequestedById { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string RefundType { get; set; }
        public string RefundMethod { get; set; }
        public decimal Amount { get; set; }
        public string? StripeRefundId { get; set; }
        public string? StripeRefundStatus { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class RequestRefundDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public string Reason { get; set; }

        [Required]
        [RegularExpression("^(Cancellation|Return)$",
            ErrorMessage = "RefundType must be Cancellation or Return.")]
        public string RefundType { get; set; }

        [Required]
        [RegularExpression("^(Card|Cash|BankTransfer)$",
            ErrorMessage = "RefundMethod must be Card, Cash or BankTransfer.")]
        public string RefundMethod { get; set; }
    }

    public class ProcessRefundDto
    {
        [Required]
        public bool IsApproved { get; set; }

        public string? AdminNotes { get; set; }
    }
}
