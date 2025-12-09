using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone] 
        [MaxLength(20)] 
        public string? PhoneNumber { get; set; }
       
    }
}