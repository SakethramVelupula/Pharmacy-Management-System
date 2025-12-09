using PharmacyManagement.DTO;

namespace PharmacyManagement.Interface
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync();
        Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto);
        Task<SupplierDto?> GetSupplierByIdAsync(int id);
        Task<SupplierDto?> UpdateSupplierAsync(int id, UpdateSupplierDto dto);
    }
}