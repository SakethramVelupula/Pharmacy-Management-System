using Microsoft.AspNetCore.Identity;
using System;

namespace PharmacyManagement.Models
{
    public class User : IdentityUser
    {
        public string Role { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsApproved { get; set; } = false;
        public string? ClinicName { get; set; }
        public string? LicenseNumber { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public string LicenseStatus { get; set; } = "Pending";
    }
}