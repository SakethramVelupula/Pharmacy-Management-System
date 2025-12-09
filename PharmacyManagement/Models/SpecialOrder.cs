using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmacyManagement.Models
{
    /// <summary>
    /// Represents a special order request made by a doctor for a drug not typically stocked.
    /// </summary>
    public class SpecialOrder
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public DateTime DateRequested { get; set; } = DateTime.UtcNow; // Default to current UTC time

        [Required]
        [MaxLength(150)]
        public string DrugName { get; set; }

        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        [Required]
        public string Reason { get; set; }

        [Required]
        [MaxLength(50)] // e.g., Pending, Approved, Rejected, Ordered, Received
        public string Status { get; set; } = "Pending"; // Default status

        public string? AdminNotes { get; set; } // Notes added by the admin during review/approval

        public DateTime? DateResolved { get; set; } // Date the request was approved/rejected

        [Required]
        public string DoctorId { get; set; } // Foreign key or identifier for the requesting doctor

        // Consider adding navigation property if you have a Doctor entity:
        // [ForeignKey("DoctorId")]
        // public virtual User RequestingDoctor { get; set; } // Assuming User model is used for Doctors

    }
}