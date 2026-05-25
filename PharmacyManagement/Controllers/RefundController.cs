using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PharmacyManagement.Controllers
{
    [ApiController]
    [Route("api/refunds")]
    [Authorize]
    public class RefundController : ControllerBase
    {
        private readonly IRefundService _refundService;

        public RefundController(IRefundService refundService)
        {
            _refundService = refundService;
        }

        [HttpPost("request")]
        [Authorize(Roles = "Doctor,Patient")]
        public async Task<ActionResult<RefundDto>> RequestRefund([FromBody] RequestRefundDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token.");

            var refund = await _refundService.RequestRefundAsync(dto, userId);
            return CreatedAtAction(nameof(GetRefundById), new { id = refund.RefundId }, refund);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<RefundDto>>> GetAllRefunds()
        {
            var refunds = await _refundService.GetAllRefundsAsync();
            return Ok(refunds);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<ActionResult<RefundDto>> GetRefundById(int id)
        {
            var refund = await _refundService.GetRefundByIdAsync(id);
            if (refund == null) return NotFound();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if ((role == "Doctor" || role == "Patient") && refund.RequestedById != userId)
                return Forbid();

            return Ok(refund);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Doctor,Patient")]
        public async Task<ActionResult<IEnumerable<RefundDto>>> GetMyRefunds()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token.");

            var refunds = await _refundService.GetRefundsByUserIdAsync(userId);
            return Ok(refunds);
        }

        [HttpPatch("{id}/process")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RefundDto>> ProcessRefund(int id, [FromBody] ProcessRefundDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var refund = await _refundService.ProcessRefundAsync(id, dto);
            return Ok(refund);
        }
    }
}
