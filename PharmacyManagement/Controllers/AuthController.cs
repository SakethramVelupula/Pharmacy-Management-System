using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace PharmacyManagement.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("register_doctor")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Doctor registration attempt for email: {Email}", model?.Email);
            var result = await _authService.RegisterDoctorAsync(model);

            if (!result.Contains("success", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { Status = "Registration Failed", Issue = result });

            return Ok(new { Status = "Success", Message = result });
        }

        [HttpPost("register_patient")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Patient registration attempt for email: {Email}", model?.Email);
            var result = await _authService.RegisterPatientAsync(model);

            if (!result.Contains("success", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { Status = "Registration Failed", Issue = result });

            return Ok(new { Status = "Success", Message = result });
        }

        // Legacy endpoint removed - use register_doctor or register_patient instead
[HttpPost("login_user")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Login attempt for email: {Email}", model?.Email);
            var token = await _authService.LoginAsync(model);

            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { Status = "Access Denied", Reason = "Authentication failed. Account may be pending approval or credentials are incorrect." });

            return Ok(new { Token = token, Status = "Login successful." });
        }

        [HttpPost("login_admin")]
        [AllowAnonymous]
        public async Task<IActionResult> AdminLogin([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _authService.LoginAdminAsync(model);

            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { Status = "Access Denied", Reason = "Authentication failed or not an admin." });

            return Ok(new { Token = token, Status = "Admin login successful." });
        }

        [HttpGet("user")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public IActionResult GetUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            return Ok(new { Status = "Authorized Access", UserId = userId, Email = userEmail, Role = userRole });
        }

        [HttpGet("pending-doctors")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingDoctors()
        {
            var pendingDoctors = await _authService.GetPendingDoctorsAsync();
            return Ok(pendingDoctors);
        }

        [HttpPut("approve-doctor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveDoctor([FromBody] ApproveDoctorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ApproveDoctorAsync(dto);

            if (result.Contains("not found") || result.Contains("not a doctor"))
                return BadRequest(new { Status = "Failed", Message = result });

            return Ok(new { Status = "Success", Message = result });
        }
    }
}
