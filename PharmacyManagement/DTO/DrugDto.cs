using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.DTO
{
    public class DrugDto
    {
        public int DrugId { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public decimal PricePerUnit { get; set; }
        public int Stock { get; set; }
        public string? StorageInstructions { get; set; }
        public bool IsPrescriptionRequired { get; set; }
        public int LowStockThreshold { get; set; }
        public bool IsLowStock => Stock <= LowStockThreshold;
    }

    public class CreateDrugDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Manufacturer { get; set; }
        [Range(0.01, 10000)]
        public decimal PricePerUnit { get; set; }
        [MaxLength(250)]
        public string? StorageInstructions { get; set; }
        public bool IsPrescriptionRequired { get; set; } = false;
        [Range(1, int.MaxValue)]
        public int LowStockThreshold { get; set; } = 10;
    }

    public class UpdateDrugDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Manufacturer { get; set; }
        [Required]
        [Range(0.01, 10000)]
        public decimal PricePerUnit { get; set; }
        [MaxLength(250)]
        public string? StorageInstructions { get; set; }
        public bool IsPrescriptionRequired { get; set; }
        [Range(1, int.MaxValue)]
        public int LowStockThreshold { get; set; } = 10;
    }
}