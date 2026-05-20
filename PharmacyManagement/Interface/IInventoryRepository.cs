using PharmacyManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface IInventoryRepository
    {
        Task<IEnumerable<Inventory>> GetAllInventoryAsync();
        Task<Inventory?> GetInventoryByDrugIdAsync(int drugId);
        Task<Inventory?> GetInventoryByDrugNameAsync(string drugName);
        Task<bool> AddDrugToInventoryAsync(string drugName, int supplierId, int quantity, DateTime? expiryDate);
        Task<bool> UpdateDrugQuantityAsync(string drugName, int newQuantity);
        Task<IEnumerable<Inventory>> GetExpiringBatchesAsync(int warningDays);
        Task<IEnumerable<Inventory>> GetExpiredBatchesAsync();
    }
}