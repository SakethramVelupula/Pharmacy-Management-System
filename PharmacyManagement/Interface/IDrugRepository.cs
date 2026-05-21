using PharmacyManagement.Models;
namespace PharmacyManagement.Interface
{
    public interface IDrugRepository
    {
        Task<IEnumerable<Drug>> GetAllAsync();
        Task<Drug?> GetByIdAsync(int id);
        Task<Drug?> GetByNameAsync(string name);
        Task<Drug> AddAsync(Drug drug);
        Task<Drug?> UpdateAsync(Drug drug);

        Task UpdateDrugStockAsync(int drugId);
        Task<IEnumerable<Drug>> GetLowStockDrugsAsync();
    }
}