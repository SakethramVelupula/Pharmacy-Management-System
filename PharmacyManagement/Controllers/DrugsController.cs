using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DTO;
using PharmacyManagement.Services;
using PharmacyManagement.Models;
using PharmacyManagement.Interface;
using Microsoft.AspNetCore.Authorization;

namespace PharmacyManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrugsController : ControllerBase
    {
        private readonly IDrugsService _service;
        public DrugsController(IDrugsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDrugs()
        {
            var drugs = await _service.GetAllDrugsAsync();
            return Ok(drugs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDrugById(int id)
        {
            var drug = await _service.GetDrugByIdAsync(id);
            if (drug == null)
            {
                return NotFound($"Drug with ID:{id} not found");
            }
            return Ok(drug);
        }
        [HttpPost]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> AddDrug([FromBody] CreateDrugDto createDrugdto)
        {
            var added = await _service.AddDrugAsync(createDrugdto);
            return CreatedAtAction(nameof(GetDrugById), new { id = added.DrugId }, added);
        }
        [HttpPut("{id}")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> UpdateDrug(int id, [FromBody] UpdateDrugDto updateDrugDto)
        {
            var updated = await _service.UpdateDrugAsync(id, updateDrugDto);
            if (updated == null)
            {
                return NotFound($"Drug with Id:{id} not found");
            }
            return Ok(updated);
        }

    }
}
