using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using System;
using System.Threading.Tasks;

namespace PharmacyManagement.Controllers
{
    [ApiController]
    [Route("api/sales")]
    [Authorize(Roles = "Admin")]
    public class SalesController : ControllerBase
    {
        private readonly ISalesService _salesService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<SalesController> _logger;

        public SalesController(ISalesService salesService, IPaymentService paymentService, ILogger<SalesController> logger)
        {
            _salesService = salesService;
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sales = await _salesService.GetAllSalesAsync();
            return Ok(sales);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var sale = await _salesService.GetSaleByIdAsync(id);
            return Ok(sale);
        }

        [HttpPost("from-order")]
        public async Task<IActionResult> CreateFromOrder([FromBody] CreateSaleDto dto)
        {
            var sale = await _salesService.CreateSaleFromOrderAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = sale.SalesId }, sale);
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var analytics = await _salesService.GetSalesAnalyticsAsync(startDate, endDate);
            return Ok(analytics);
        }

        [HttpPost("payment-intent/{orderId}")]
        public async Task<IActionResult> CreatePaymentIntent(int orderId)
        {
            var result = await _paymentService.CreatePaymentIntentAsync(orderId);
            return Ok(result);
        }

        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentDto dto)
        {
            var sale = await _paymentService.ConfirmPaymentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = sale.SalesId }, sale);
        }
    }
}