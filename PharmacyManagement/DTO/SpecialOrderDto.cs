using System;
using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.DTO
{
    /// <summary>
    /// DTO for reading Special Order data.
    /// </summary>
    public class SpecialOrderDto
    {
        public int RequestId { get; set; }
        public DateTime DateRequested { get; set; }
        public string DrugName { get; set; }
        public string? Manufacturer { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime? DateResolved { get; set; }
        public string DoctorId { get; set; }
        // Add DoctorName if needed, requires joining or mapping logic
        // public string DoctorName { get; set; }
    }

    public class CreateSpecialOrderDto
    {
        [Required(ErrorMessage = "Drug Name is required.")]
        [MaxLength(150, ErrorMessage = "Drug Name cannot exceed 150 characters.")]
        public string DrugName { get; set; }

        [MaxLength(100, ErrorMessage = "Manufacturer cannot exceed 100 characters.")]
        public string? Manufacturer { get; set; }

        [Required(ErrorMessage = "Reason for the special order is required.")]
        public string Reason { get; set; }
    }

    public class UpdateSpecialOrderStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
       
        public string Status { get; set; } // e.g., "Approved", "Rejected", "Ordered"

        public string? AdminNotes { get; set; } 
    }
}
