using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PharmacyManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;
        private readonly IMapper _mapper;
        private readonly int _licenseWarningDays;

        public AuthService(IAuthRepository authRepository, IEmailService emailService, IAuditService auditService, IMapper mapper, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _emailService = emailService;
            _auditService = auditService;
            _mapper = mapper;
            _licenseWarningDays = configuration.GetValue<int>("LicenseExpiry:WarningDays", 30);
        }
        public async Task<string> RegisterDoctorAsync(RegisterDoctorDto model)
        {
            if (model.LicenseExpiryDate.Date <= DateTime.UtcNow.Date)
                return "Registration failed: License expiry date must be in the future.";

            var licenseStatus = model.LicenseExpiryDate.Date <= DateTime.UtcNow.AddDays(_licenseWarningDays).Date
                ? "ExpiringSoon" : "Valid";

            var user = new User
            {
                UserName = model.Name,
                Email = model.Email,
                Role = "Doctor",
                IsApproved = false,
                ClinicName = model.ClinicName,
                LicenseNumber = model.LicenseNumber,
                LicenseExpiryDate = model.LicenseExpiryDate,
                LicenseStatus = licenseStatus
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

        public async Task<string?> LoginAsync(LoginDto model)
        {
            var result = await _authRepository.LoginAsync(model.Email, model.Password);
            if (result != null)
            {
                var user = await _authRepository.GetUserByEmailAsync(model.Email);
                if (user != null)
                    await _auditService.LogAsync("User", user.Id, "Login", user.Id,
                        $"User '{user.UserName}' logged in.");
            }
            return result;
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

            await _auditService.LogAsync("User", doctor.Id, dto.IsApproved ? "Approved" : "Rejected", dto.DoctorId,
                $"Doctor '{doctor.UserName}' was {(dto.IsApproved ? "approved" : "rejected")}.");

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

        public async Task<LicenseValidationResultDto> ValidateLicenseAsync(string doctorId)
        {
            var doctor = await _authRepository.GetUserByIdAsync(doctorId);
            if (doctor == null || doctor.Role != "Doctor")
                return new LicenseValidationResultDto { IsValid = false, Status = "NotFound", Message = "Doctor not found." };

            if (string.IsNullOrEmpty(doctor.LicenseNumber))
                return new LicenseValidationResultDto { IsValid = false, Status = "Missing", Message = "No license number on record." };

            if (!doctor.LicenseExpiryDate.HasValue)
                return new LicenseValidationResultDto { IsValid = false, LicenseNumber = doctor.LicenseNumber, Status = "NoExpiry", Message = "No expiry date on record." };

            var daysUntilExpiry = (doctor.LicenseExpiryDate.Value.Date - DateTime.UtcNow.Date).Days;

            if (daysUntilExpiry < 0)
            {
                doctor.LicenseStatus = "Expired";
                await _authRepository.UpdateUserAsync(doctor);
                return new LicenseValidationResultDto { IsValid = false, LicenseNumber = doctor.LicenseNumber, Status = "Expired", Message = "License has expired.", ExpiryDate = doctor.LicenseExpiryDate, DaysUntilExpiry = daysUntilExpiry };
            }

            if (daysUntilExpiry <= _licenseWarningDays)
            {
                doctor.LicenseStatus = "ExpiringSoon";
                await _authRepository.UpdateUserAsync(doctor);
                return new LicenseValidationResultDto { IsValid = true, LicenseNumber = doctor.LicenseNumber, Status = "ExpiringSoon", Message = $"License expires in {daysUntilExpiry} days.", ExpiryDate = doctor.LicenseExpiryDate, DaysUntilExpiry = daysUntilExpiry };
            }

            doctor.LicenseStatus = "Valid";
            await _authRepository.UpdateUserAsync(doctor);
            return new LicenseValidationResultDto { IsValid = true, LicenseNumber = doctor.LicenseNumber, Status = "Valid", Message = "License is valid.", ExpiryDate = doctor.LicenseExpiryDate, DaysUntilExpiry = daysUntilExpiry };
        }

        public async Task<IEnumerable<ExpiringLicenseDto>> GetExpiringLicensesAsync(int? warningDays = null)
        {
            var days = warningDays ?? _licenseWarningDays;
            var doctors = await _authRepository.GetDoctorsWithExpiringLicensesAsync(days);
            return doctors.Select(d => new ExpiringLicenseDto
            {
                DoctorId = d.Id,
                Name = d.UserName ?? "Unknown",
                Email = d.Email ?? "Unknown",
                LicenseNumber = d.LicenseNumber ?? "N/A",
                LicenseExpiryDate = d.LicenseExpiryDate!.Value,
                DaysUntilExpiry = (d.LicenseExpiryDate.Value.Date - DateTime.UtcNow.Date).Days
            });
        }
    }
}