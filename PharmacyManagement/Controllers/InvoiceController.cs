using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Interface;
using System.Security.Claims;

namespace PharmacyManagement.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    [Authorize]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IOrderService _orderService;

        public InvoiceController(IInvoiceService invoiceService, IOrderService orderService)
        {
            _invoiceService = invoiceService;
            _orderService = orderService;
        }

        [HttpGet("{orderId}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> DownloadInvoice(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound("Order not found.");

            // Doctors and patients can only download their own invoices
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if ((role == "Doctor" || role == "Patient") && order.PlacedById != userId)
                return Forbid();

            var pdfBytes = await _invoiceService.GenerateInvoiceAsync(orderId);
            return File(pdfBytes, "application/pdf", $"Invoice-Order-{orderId}.pdf");
        }
    }
}
