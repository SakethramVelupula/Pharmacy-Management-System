using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;

namespace PharmacyManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _service;

        public SuppliersController(ISupplierService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSuppliers()
        {
            var suppliers = await _service.GetAllSuppliersAsync();
            return Ok(suppliers);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var supplier = await _service.CreateSupplierAsync(dto);
            return Ok(supplier);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var supplier = await _service.UpdateSupplierAsync(id, dto);
            if (supplier == null) return NotFound();
            return Ok(supplier);
        }
    }
}