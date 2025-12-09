using PharmacyManagement.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface IInventoryService
    {
        Task<IEnumerable<ReadInventoryDto>> GetAllInventoryAsync();
        Task<ReadInventoryDto?> GetInventoryByDrugIdAsync(int drugId);
        Task<ReadInventoryDto?> GetInventoryByDrugNameAsync(string drugName);
        Task<bool> AddDrugToInventoryAsync(AddInventoryDto dto);
        Task<bool> UpdateDrugQuantityAsync(string drugName, UpdateInventoryQuantityDto dto);

    }
}