
using PharmacyManagement.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface ISpecialOrderService
    {
        Task<SpecialOrderDto?> GetByIdAsync(int id);
        Task<IEnumerable<SpecialOrderDto>> GetAllAsync();
        Task<IEnumerable<SpecialOrderDto>> GetByDoctorIdAsync(string doctorId);
        Task<SpecialOrderDto> CreateAsync(CreateSpecialOrderDto dto, string userId);
        Task<bool> UpdateStatusAsync(int id, UpdateSpecialOrderStatusDto dto);
    }
}