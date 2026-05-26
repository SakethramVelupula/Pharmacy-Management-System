using PharmacyManagement.DTO;
using PharmacyManagement.Models;
namespace PharmacyManagement.Interface
{
    public interface IDrugsService
    {
        Task<IEnumerable<DrugDto>> GetAllDrugsAsync();
        Task<DrugDto?> GetDrugByIdAsync(int id);
        Task<DrugDto> AddDrugAsync(CreateDrugDto createdrugDto, string performedById);
        Task<DrugDto?> UpdateDrugAsync(int id, UpdateDrugDto updatedrugdto, string performedById);

        Task UpdateDrugStockAsync(int drugId);
        Task<Drug?> GetByNameAsync(string name);
        Task<IEnumerable<DrugDto>> GetLowStockDrugsAsync();
    }
}