using System.ComponentModel.DataAnnotations;
namespace PharmacyManagement.DTO
{
    public class LoginDto
    {
        [EmailAddress]
        public string Email { get; set; }
        
        public string Password { get; set; }
    }
}