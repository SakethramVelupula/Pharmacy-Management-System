using PharmacyManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface ISpecialOrderRepository
    {
        Task<SpecialOrder?> GetByIdAsync(int requestId);
        Task<IEnumerable<SpecialOrder>> GetAllAsync();
        Task<IEnumerable<SpecialOrder>> GetByDoctorIdAsync(string doctorId);
        Task<SpecialOrder> CreateAsync(SpecialOrder specialOrder);
        Task<bool> UpdateStatusAsync(int requestId, string status, string? adminNotes = null);
    }
}