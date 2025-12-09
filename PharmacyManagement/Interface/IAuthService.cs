using PharmacyManagement.DTO;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto model);
        Task<string?> LoginAsync(LoginDto model);
        Task<string?> LoginAdminAsync(LoginDto model);


    }
}