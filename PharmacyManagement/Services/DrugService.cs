using AutoMapper;
using PharmacyManagement.DTO;
using PharmacyManagement.Interface;
using PharmacyManagement.Models;
using PharmacyManagement.Repository;
namespace PharmacyManagement.Services
{
    public class DrugService : IDrugsService
    {
        private readonly IDrugRepository _repo;
        private readonly IMapper _mapper;
        private readonly IAuditService _auditService;

        public DrugService(IDrugRepository repo, IMapper mapper, IAuditService auditService)
        {
            _repo = repo;
            _mapper = mapper;
            _auditService = auditService;
        }
        public async Task<IEnumerable<DrugDto>> GetAllDrugsAsync()
        {
            var drugs = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<DrugDto>>(drugs);
        }
        public async Task<DrugDto?> GetDrugByIdAsync(int id)
        {
            var drug = await _repo.GetByIdAsync(id);
            return drug == null ? null : _mapper.Map<DrugDto>(drug);
        }
        public async Task<DrugDto> AddDrugAsync(CreateDrugDto createDrugDto, string performedById)
        {
            var drug = _mapper.Map<Drug>(createDrugDto);
            drug.Stock = 0;
            var added = await _repo.AddAsync(drug);
            await UpdateDrugStockAsync(added.DrugId);
            await _auditService.LogAsync("Drug", added.DrugId.ToString(), "Created", performedById,
                $"Drug '{added.Name}' created by admin.");
            return _mapper.Map<DrugDto>(added);
        }
        public async Task<DrugDto?> UpdateDrugAsync(int id, UpdateDrugDto updateDrugDto, string performedById)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;
            var currentStock = existing.Stock;
            _mapper.Map(updateDrugDto, existing);
            existing.Stock = currentStock;
            var updated = await _repo.UpdateAsync(existing);
            await _auditService.LogAsync("Drug", id.ToString(), "Updated", performedById,
                $"Drug '{existing.Name}' updated by admin.");
            return _mapper.Map<DrugDto>(updated);
        }

        public async Task UpdateDrugStockAsync(int drugId)
        {
            await _repo.UpdateDrugStockAsync(drugId);
        }
        public async Task<Drug?> GetByNameAsync(string name)
        {
            return await _repo.GetByNameAsync(name);
        }

        public async Task<IEnumerable<DrugDto>> GetLowStockDrugsAsync()
        {
            var drugs = await _repo.GetLowStockDrugsAsync();
            return _mapper.Map<IEnumerable<DrugDto>>(drugs);
        }
    }
}