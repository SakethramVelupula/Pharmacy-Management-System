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
        public DrugService(IDrugRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
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
        public async Task<DrugDto> AddDrugAsync(CreateDrugDto createDrugDto)
        {
            var drug = _mapper.Map<Drug>(createDrugDto);
            drug.Stock = 0; // Stock starts at 0
            var added = await _repo.AddAsync(drug);
            
            // Sync stock from existing inventory if any
            await UpdateDrugStockAsync(added.DrugId);
            
            return _mapper.Map<DrugDto>(added);
        }
        public async Task<DrugDto?> UpdateDrugAsync(int id, UpdateDrugDto updateDrugDto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }
            var currentStock = existing.Stock; // Preserve current stock
            _mapper.Map(updateDrugDto, existing);
            existing.Stock = currentStock; // Restore stock value
            var updated = await _repo.UpdateAsync(existing);
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
    }
}