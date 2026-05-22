using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.DTO
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class RegisterDoctorDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ClinicName { get; set; }
        [Required]
        [RegularExpression(@"^[A-Z]{2,4}-\d{4,8}$",
            ErrorMessage = "License number must be in format: XX-12345 (2-4 uppercase letters, hyphen, 4-8 digits). e.g. MED-12345")]
        public string LicenseNumber { get; set; }
        [Required]
        public DateTime LicenseExpiryDate { get; set; }
    }

    public class LicenseValidationResultDto
    {
        public bool IsValid { get; set; }
        public string LicenseNumber { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? DaysUntilExpiry { get; set; }
    }

    public class ExpiringLicenseDto
    {
        public string DoctorId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string LicenseNumber { get; set; }
        public DateTime LicenseExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
    }

    public class RegisterPatientDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class ApproveDoctorDto
    {
        [Required]
        public string DoctorId { get; set; }
        [Required]
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class PendingDoctorDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ClinicName { get; set; }
        public string LicenseNumber { get; set; }
    }
}
