using PharmacyManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface ISalesRepository
    {
        Task<IEnumerable<Sales>> GetAllAsync();
        Task<Sales?> GetByIdAsync(int id);
        Task<Sales> CreateAsync(Sales sale);
        Task<Sales?> UpdateAsync(int id, Sales sale);
        Task<bool> DeleteAsync(int id);
    }
}