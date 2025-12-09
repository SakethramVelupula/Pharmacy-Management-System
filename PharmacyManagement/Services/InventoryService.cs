using AutoMapper;
using Microsoft.Extensions.Logging;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _repo;
        private readonly IDrugsService _drugService;
        private readonly IMapper _mapper;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(IInventoryRepository repo,IDrugsService drugService, IMapper mapper, ILogger<InventoryService> logger)
        {
            _repo = repo;
             _drugService=drugService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ReadInventoryDto>> GetAllInventoryAsync()
        {
            var data = await _repo.GetAllInventoryAsync();
            return _mapper.Map<IEnumerable<ReadInventoryDto>>(data);
        }

        public async Task<ReadInventoryDto?> GetInventoryByDrugIdAsync(int drugId)
        {
            var data = await _repo.GetInventoryByDrugIdAsync(drugId);
            return _mapper.Map<ReadInventoryDto?>(data);
        }

        public async Task<ReadInventoryDto?> GetInventoryByDrugNameAsync(string drugName)
        {
            var data = await _repo.GetInventoryByDrugNameAsync(drugName);
            return _mapper.Map<ReadInventoryDto?>(data);
        }

        public async Task<bool> AddDrugToInventoryAsync(AddInventoryDto dto)
        {
            _logger.LogInformation("Service: Adding inventory for drug {DrugName}", dto.DrugName);
            var result = await _repo.AddDrugToInventoryAsync(dto.DrugName, dto.SupplierId, dto.Quantity);
            var drug = await _drugService.GetByNameAsync(dto.DrugName);
            if (drug != null)
            {
                await _drugService.UpdateDrugStockAsync(drug.DrugId);
            }
            return result;
        }

        public async Task<bool> UpdateDrugQuantityAsync(string drugName, UpdateInventoryQuantityDto dto)
        {
            _logger.LogInformation("Service: Updating quantity for drug {DrugName}", drugName);
            var result = await _repo.UpdateDrugQuantityAsync(drugName, dto.NewQuantity);
            var drug = await _drugService.GetByNameAsync(drugName);
            if (drug != null)
            {
                await _drugService.UpdateDrugStockAsync(drug.DrugId);
            }
            return result;
        }

    }
}