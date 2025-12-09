using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.DTO
{
    public class SupplierDto
    {
        public int SupplierId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class CreateSupplierDto
    {
        [Required]
        public string Name { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class UpdateSupplierDto
    {
        [Required]
        public string Name { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}