using System;
using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.DTO
{
    
    public class OrderDto
    {
        public int Id { get; set; }
        public string DoctorId { get; set; }
        public int DrugId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public DateTime PlacedAt { get; set; }

       
        public string? PrescriptionReference { get; set; }
        public DateTime? DateDispensed { get; set; }
        
    }

    
    public class CreateOrderDto
    {
        [Required]
        public int DrugId { get; set; }
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        
        [MaxLength(100)]
        public string? PrescriptionReference { get; set; }
        
    }

   
    public class UpdateOrderDto
    {
        [Range(1, int.MaxValue)] 
        public int Quantity { get; set; }
        [Required]
        public string Status { get; set; }

       
        [MaxLength(100)]
        public string? PrescriptionReference { get; set; }
        public DateTime? DateDispensed { get; set; } 
       
    }
}