using Microsoft.AspNetCore.Mvc;

namespace PharmacyManagement.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Application = "Pharmacy Management System"
            });
        }
    }
}
