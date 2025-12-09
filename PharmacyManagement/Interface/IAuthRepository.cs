using PharmacyManagement.Models;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface IAuthRepository
    {
        Task<(bool IsSuccess, string Message)> RegisterAsync(User user, string password);
        Task<string?> LoginAsync(string email, string password);
        Task<User?> GetUserByEmailAsync(string email);
    }
}