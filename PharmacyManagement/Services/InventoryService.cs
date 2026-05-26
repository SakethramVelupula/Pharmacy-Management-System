using AutoMapper;
using Microsoft.Extensions.Configuration;
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
        private readonly IAuditService _auditService;
        private readonly int _expiryWarningDays;

        public InventoryService(IInventoryRepository repo, IDrugsService drugService, IMapper mapper, ILogger<InventoryService> logger, IConfiguration configuration, IAuditService auditService)
        {
            _repo = repo;
            _drugService = drugService;
            _mapper = mapper;
            _logger = logger;
            _auditService = auditService;
            _expiryWarningDays = configuration.GetValue<int>("DrugExpiry:ExpiryWarningDays", 90);
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

        public async Task<bool> AddDrugToInventoryAsync(AddInventoryDto dto, string performedById)
        {
            _logger.LogInformation("Service: Adding inventory for drug {DrugName}", dto.DrugName);
            var result = await _repo.AddDrugToInventoryAsync(dto.DrugName, dto.SupplierId, dto.Quantity, dto.ExpiryDate);
            var drug = await _drugService.GetByNameAsync(dto.DrugName);
            if (drug != null)
                await _drugService.UpdateDrugStockAsync(drug.DrugId);
            await _auditService.LogAsync("Inventory", null, "Created", performedById,
                $"Inventory added for drug '{dto.DrugName}', quantity: {dto.Quantity}.");
            return result;
        }

        public async Task<bool> UpdateDrugQuantityAsync(string drugName, UpdateInventoryQuantityDto dto, string performedById)
        {
            _logger.LogInformation("Service: Updating quantity for drug {DrugName}", drugName);
            var result = await _repo.UpdateDrugQuantityAsync(drugName, dto.QuantityToAdd);
            var drug = await _drugService.GetByNameAsync(drugName);
            if (drug != null)
                await _drugService.UpdateDrugStockAsync(drug.DrugId);
            await _auditService.LogAsync("Inventory", null, "Updated", performedById,
                $"Inventory updated for drug '{drugName}', added quantity: {dto.QuantityToAdd}.");
            return result;
        }

        public async Task<IEnumerable<ExpiringBatchDto>> GetExpiringBatchesAsync(int? warningDays = null)
        {
            var days = warningDays ?? _expiryWarningDays;
            var batches = await _repo.GetExpiringBatchesAsync(days);
            return batches.Select(ToExpiringBatchDto);
        }

        public async Task<IEnumerable<ExpiringBatchDto>> GetExpiredBatchesAsync()
        {
            var batches = await _repo.GetExpiredBatchesAsync();
            return batches.Select(ToExpiringBatchDto);
        }

        private static ExpiringBatchDto ToExpiringBatchDto(PharmacyManagement.Models.Inventory i) => new()
        {
            InventoryId = i.Id,
            DrugId = i.DrugId,
            DrugName = i.Drug?.Name ?? "Unknown",
            Quantity = i.Quantity,
            ExpiryDate = i.ExpiryDate!.Value,
            DaysUntilExpiry = (i.ExpiryDate.Value.Date - DateTime.UtcNow.Date).Days,
            IsExpired = i.ExpiryDate.Value.Date <= DateTime.UtcNow.Date
        };
    }
}