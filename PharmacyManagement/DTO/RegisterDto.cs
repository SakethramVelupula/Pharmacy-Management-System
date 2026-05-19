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
        public string LicenseNumber { get; set; }
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
