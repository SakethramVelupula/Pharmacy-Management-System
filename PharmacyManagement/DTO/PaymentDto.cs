using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.DTO
{
    public class PaymentIntentResponseDto
    {
        public string PaymentIntentId { get; set; }
        public string ClientSecret { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
    }

    public class ConfirmPaymentDto
    {
        [Required]
        public int OrderId { get; set; }
        [Required]
        public string PaymentIntentId { get; set; }
        public string PaymentMethod { get; set; } = "Card";
    }

    public class PaymentTransactionDto
    {
        public int Id { get; set; }
        public string PaymentIntentId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
