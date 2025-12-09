using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.DTO
{
    public class DrugDto
    {
        public int DrugId { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? StorageInstructions { get; set; }
        public bool IsPrescriptionRequired { get; set; }
       
    }

    public class CreateDrugDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Manufacturer { get; set; }
        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        [MaxLength(250)]
        public string? StorageInstructions { get; set; }
        public bool IsPrescriptionRequired { get; set; } = false;
        
    } 

    public class UpdateDrugDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Manufacturer { get; set; }
        [Required]
        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        [MaxLength(250)]
        public string? StorageInstructions { get; set; }
        public bool IsPrescriptionRequired { get; set; }
    }
}