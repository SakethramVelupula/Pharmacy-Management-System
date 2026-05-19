using PharmacyManagement.DTO;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface IAuthService
    {
        Task<string> RegisterDoctorAsync(RegisterDoctorDto model);
        Task<string> RegisterPatientAsync(RegisterPatientDto model);
        Task<string?> LoginAsync(LoginDto model);
        Task<string?> LoginAdminAsync(LoginDto model);
        Task<string> ApproveDoctorAsync(ApproveDoctorDto dto);
        Task<IEnumerable<PendingDoctorDto>> GetPendingDoctorsAsync();
    }
}