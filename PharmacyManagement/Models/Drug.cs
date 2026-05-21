using System;
using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.Models
{
    public class Drug
    {
        [Key]
        public int DrugId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Required]
        public string Manufacturer { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int Stock { get; set; }
        [MaxLength(250)]
        public string? StorageInstructions { get; set; }
        public bool IsPrescriptionRequired { get; set; } = false;
        public int LowStockThreshold { get; set; } = 10;
    }
}