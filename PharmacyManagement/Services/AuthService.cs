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

        public async Task<string> RegisterAsync(RegisterDto model)
        {
            var user = _mapper.Map<User>(model);
            user.UserName = model.Name;
            user.Role = "Doctor";
            var (isSuccess, message) = await _authRepository.RegisterAsync(user, model.Password);

            if (isSuccess)
                await _emailService.SendWelcomeEmailAsync(model.Email, model.Name);

            return isSuccess ? "User registered successfully." : $"Registration failed: {message}";
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
        


    }
}