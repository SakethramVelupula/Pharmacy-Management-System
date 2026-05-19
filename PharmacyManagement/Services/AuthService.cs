using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using AutoMapper;
using System.Threading.Tasks;

namespace PharmacyManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public AuthService(IAuthRepository authRepository, IEmailService emailService, IMapper mapper)
        {
            _authRepository = authRepository;
            _emailService = emailService;
            _mapper = mapper;
        }
        public async Task<string> RegisterDoctorAsync(RegisterDoctorDto model)
        {
            var user = new User
            {
                UserName = model.Name,
                Email = model.Email,
                Role = "Doctor",
                IsApproved = false,
                ClinicName = model.ClinicName,
                LicenseNumber = model.LicenseNumber
            };

            var (isSuccess, message) = await _authRepository.RegisterAsync(user, model.Password);

            if (isSuccess)
            {
                await _emailService.SendDoctorPendingApprovalAsync(model.Email, model.Name);
                await _emailService.SendAdminNewDoctorRegistrationAsync(model.Name, model.Email, model.ClinicName, model.LicenseNumber);
            }

            return isSuccess ? "Doctor registration successful. Pending admin approval." : $"Registration failed: {message}";
        }

        public async Task<string> RegisterPatientAsync(RegisterPatientDto model)
        {
            var user = new User
            {
                UserName = model.Name,
                Email = model.Email,
                Role = "Patient",
                IsApproved = true // Patients are auto-approved
            };

            var (isSuccess, message) = await _authRepository.RegisterAsync(user, model.Password);

            if (isSuccess)
                await _emailService.SendWelcomeEmailAsync(model.Email, model.Name);

            return isSuccess ? "Patient registration successful." : $"Registration failed: {message}";
        }

        public Task<string?> LoginAsync(LoginDto model)
        {
            return _authRepository.LoginAsync(model.Email, model.Password);
        }

        public async Task<string?> LoginAdminAsync(LoginDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email))
                return null;

            var user = await _authRepository.GetUserByEmailAsync(model.Email);
            if (user?.Role != "Admin")
                return null;

            return await _authRepository.LoginAsync(model.Email, model.Password);
        }

        public async Task<string> ApproveDoctorAsync(ApproveDoctorDto dto)
        {
            var doctor = await _authRepository.GetUserByIdAsync(dto.DoctorId);
            if (doctor == null)
                return "Doctor not found.";

            if (doctor.Role != "Doctor")
                return "User is not a doctor.";

            doctor.IsApproved = dto.IsApproved;
            await _authRepository.UpdateUserAsync(doctor);

            if (dto.IsApproved)
                await _emailService.SendDoctorApprovedAsync(doctor.Email!, doctor.UserName!);
            else
                await _emailService.SendDoctorRejectedAsync(doctor.Email!, doctor.UserName!, dto.RejectionReason ?? "No reason provided.");

            return dto.IsApproved ? "Doctor approved successfully." : "Doctor rejected successfully.";
        }

        public async Task<IEnumerable<PendingDoctorDto>> GetPendingDoctorsAsync()
        {
            var pendingDoctors = await _authRepository.GetPendingDoctorsAsync();
            return pendingDoctors.Select(d => new PendingDoctorDto
            {
                Id = d.Id,
                Name = d.UserName ?? "Unknown",
                Email = d.Email ?? "Unknown",
                ClinicName = d.ClinicName ?? "N/A",
                LicenseNumber = d.LicenseNumber ?? "N/A"
            });
        }
    }
}
