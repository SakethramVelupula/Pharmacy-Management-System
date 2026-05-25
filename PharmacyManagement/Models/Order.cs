using System; 
using System.ComponentModel.DataAnnotations; 

namespace PharmacyManagement.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string PlacedById { get; set; }
        public int DrugId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = "Pending"; // Pending / Processing / Delivered / Cancelled / Returned
        public DateTime PlacedAt { get; set; } = DateTime.Now;

        public Drug Drug { get; set; }

       
        [MaxLength(100)]
        public string? PrescriptionReference { get; set; }
        public DateTime? DateDispensed { get; set; }

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Cash";
    }
}