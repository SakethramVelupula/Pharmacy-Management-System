using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using System.Security.Claims;

namespace PharmacyManagement.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("view")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (role == "Doctor" || role == "Patient")
            {
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User ID not found in token");

                var orders = await _orderService.GetOrdersByUserIdAsync(userId);
                return Ok(orders);
            }

            var allOrders = await _orderService.GetAllOrdersAsync();
            return Ok(allOrders);
        }

        [HttpGet("view/{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if ((role == "Doctor" || role == "Patient") && order.PlacedById != userId)
                return Forbid();

            return Ok(order);
        }

        // Doctor places an order (prescription optional)
        [HttpPost("doctor")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<OrderDto>> CreateDoctorOrder([FromBody] CreateOrderDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var created = await _orderService.CreateOrderAsync(dto, userId);
            return CreatedAtAction(nameof(GetOrder), new { id = created.Id }, created);
        }

        // Patient places an order (prescription required)
        [HttpPost("patient")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<OrderDto>> CreatePatientOrder([FromBody] CreatePatientOrderDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var created = await _orderService.CreatePatientOrderAsync(dto, userId);
            return CreatedAtAction(nameof(GetOrder), new { id = created.Id }, created);
        }

        [HttpPatch("update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderDto>> UpdateOrder(int id, [FromBody] UpdateOrderDto dto)
        {
            var updated = await _orderService.UpdateOrderAsync(id, dto);
            return Ok(updated);
        }

        [HttpPatch("status/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var validStatuses = new[] { "Pending", "Processing", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(status))
                return BadRequest($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            var updateDto = new UpdateOrderDto
            {
                Status = status,
                Quantity = order.Quantity,
                PrescriptionReference = order.PrescriptionReference,
                DateDispensed = order.DateDispensed
            };
            var updated = await _orderService.UpdateOrderAsync(id, updateDto);
            return Ok(updated);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            await _orderService.DeleteOrderAsync(id);
            return NoContent();
        }
    }
}
