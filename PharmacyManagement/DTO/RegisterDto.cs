using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.DTO
{
    public class RegisterDto
    {
        [EmailAddress]
        public string Email { get; set; }
        public string Name {get; set;} 
        public string Password { get; set; }
        //public string Role { get; set; } 
    }
}