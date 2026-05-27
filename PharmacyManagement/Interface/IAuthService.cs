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
        Task<LicenseValidationResultDto> ValidateLicenseAsync(string doctorId);
        Task<IEnumerable<ExpiringLicenseDto>> GetExpiringLicensesAsync(int? warningDays = null);
        Task<string> ForgotPasswordAsync(string email);
        Task<string> ResetPasswordAsync(ResetPasswordDto dto);
    }
}