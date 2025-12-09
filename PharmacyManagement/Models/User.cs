using Microsoft.AspNetCore.Identity;
using System;

namespace PharmacyManagement.Models
{
    public class User : IdentityUser
    {
        public string Role { get; set; } 

        
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; } = true; 
        
    }
}