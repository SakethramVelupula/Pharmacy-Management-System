using System;
using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.DTO
{
    
    public class AddInventoryDto
    {
        [Required]
        public string DrugName { get; set; }
        [Required]
        public int SupplierId { get; set; }
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class ReadInventoryDto
    {
        public int Id { get; set; }
        public int DrugId { get; set; }
        public string DrugName { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int Quantity { get; set; }
        public DateTime? LastRestockDate { get; set; }
        public string? DrugStorageInstructions { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value.Date < DateTime.UtcNow.Date;
        public bool IsExpiringSoon { get; set; }
    }

    public class ExpiringBatchDto
    {
        public int InventoryId { get; set; }
        public int DrugId { get; set; }
        public string DrugName { get; set; }
        public int Quantity { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public bool IsExpired { get; set; }
    }

   
    public class UpdateInventoryQuantityDto
    {
        [Range(0, int.MaxValue)] 
        public int QuantityToAdd { get; set; }
       
    }
}