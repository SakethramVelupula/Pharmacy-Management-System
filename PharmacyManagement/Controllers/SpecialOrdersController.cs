using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PharmacyManagement.Controllers
{
    [Route("api/special-orders")]
    [ApiController]
    [Authorize]
    public class SpecialOrdersController : ControllerBase
    {
        private readonly ISpecialOrderService _service;

        public SpecialOrdersController(ISpecialOrderService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<SpecialOrderDto>> CreateSpecialOrder([FromBody] CreateSpecialOrderDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var createdOrder = await _service.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetSpecialOrderById), new { id = createdOrder.RequestId }, createdOrder);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<IEnumerable<SpecialOrderDto>>> GetSpecialOrders([FromQuery] string? status = null, [FromQuery] string? doctorId = null)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (role == "Doctor")
            {
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token");
                    
                var orders = await _service.GetByDoctorIdAsync(userId);
                return Ok(orders);
            }

            var allOrders = await _service.GetAllAsync();
            if (!string.IsNullOrEmpty(status))
                allOrders = allOrders.Where(o => o.Status.Equals(status, System.StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(doctorId))
                allOrders = allOrders.Where(o => o.DoctorId == doctorId);

            return Ok(allOrders);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<SpecialOrderDto>> GetSpecialOrderById(int id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (role == "Doctor" && order.DoctorId != userId)
                return Forbid();

            return Ok(order);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateSpecialOrderStatusDto dto)
        {
            var success = await _service.UpdateStatusAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}