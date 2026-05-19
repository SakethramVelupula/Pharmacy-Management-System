using PharmacyManagement.Models;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface IAuthRepository
    {
        Task<(bool IsSuccess, string Message)> RegisterAsync(User user, string password);
        Task<string?> LoginAsync(string email, string password);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(string userId);
        Task<IEnumerable<User>> GetPendingDoctorsAsync();
        Task<bool> UpdateUserAsync(User user);
    }
}