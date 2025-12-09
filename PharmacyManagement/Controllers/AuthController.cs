
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Azure.Identity;

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

        [HttpPost("register_user")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Register endpoint called with invalid model state.");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Attempting registration for email: {Email}", model?.Email);

            var result = await _authService.RegisterAsync(model);

            if (!result.Contains("success", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Registration failed for email {Email}: {Result}", model.Email, result);
                return BadRequest(new { Status = "Registration Failed", Issue = result });
            }

            _logger.LogInformation("Registration successful for email: {Email}", model.Email);
            return Ok(new { Status = "Success", ResultInfo = result });
        }

        [HttpPost("login_user")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login endpoint called with invalid model state for email: {Email}", model?.Email);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Attempting login for email: {Email}", model?.Email);

            var token = await _authService.LoginAsync(model);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Login failed for email {Email} (Invalid credentials or inactive account).", model.Email);
                return Unauthorized(new { Status = "Access Denied", Reason = "Authentication failed. Please check login details or account status." });
            }

            _logger.LogInformation("Login successful for email: {Email}", model.Email);
            return Ok(new { Token = token, Status = "Login successful." });
        }

        [HttpGet("user")]
        [Authorize(Roles ="Admin,Doctor")]
        public IActionResult GetUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Fetching user info for authenticated user ID: {UserId}", userId);

            if (userId == null)
            {
                _logger.LogError("GetUser called but user ID claim (sub) is missing from token.");
                return Unauthorized();
            }
            //var userName = User.Identity?.Name;
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            return Ok(new { Status = "Authorized Access", UserId = userId, Email = userEmail, Role = userRole });
        }

        [HttpPost("login_admin")]
        [AllowAnonymous]
        public async Task<IActionResult> AdminLogin([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Admin login called with invalid model state for email: {Email}", model?.Email);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Attempting admin login for email: {Email}", model?.Email);

            var token = await _authService.LoginAdminAsync(model);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Admin login failed for email {Email}", model.Email);
                return Unauthorized(new { Status = "Access Denied", Reason = "Authentication failed or not an admin." });
            }

            _logger.LogInformation("Admin login successful for email: {Email}", model.Email);
            return Ok(new { Token = token, Status = "Admin login successful." });
        }

    }
}
