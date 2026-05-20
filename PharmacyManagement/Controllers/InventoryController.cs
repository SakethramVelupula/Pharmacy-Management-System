using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Controllers
{
    [Route("api/inventory")]
    [ApiController]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _service;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IInventoryService service, ILogger<InventoryController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("view")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<IEnumerable<ReadInventoryDto>>> GetAllInventory()
        {
            var result = await _service.GetAllInventoryAsync();
            return Ok(result);
        }

        [HttpGet("byId/{drugId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<ReadInventoryDto>> GetById(int drugId)
        {
            var result = await _service.GetInventoryByDrugIdAsync(drugId);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("byName/{drugName}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<ReadInventoryDto>> GetByName(string drugName)
        {
            var result = await _service.GetInventoryByDrugNameAsync(drugName);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddInventory([FromBody] AddInventoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _service.AddDrugToInventoryAsync(dto);
                return Ok(new { Message = $"Inventory for {dto.DrugName} added/updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding inventory for {DrugName}", dto.DrugName);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPatch("update/{drugName}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateQuantity(string drugName, [FromBody] UpdateInventoryQuantityDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _service.UpdateDrugQuantityAsync(drugName, dto);
            return Ok(new { Message = $"Inventory quantity updated for {drugName}." });
        }

        [HttpGet("expiring")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ExpiringBatchDto>>> GetExpiringBatches([FromQuery] int? days = null)
        {
            var batches = await _service.GetExpiringBatchesAsync(days);
            return Ok(batches);
        }

        [HttpGet("expired")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ExpiringBatchDto>>> GetExpiredBatches()
        {
            var batches = await _service.GetExpiredBatchesAsync();
            return Ok(batches);
        }
    }
}