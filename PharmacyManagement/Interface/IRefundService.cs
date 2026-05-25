using PharmacyManagement.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface IRefundService
    {
        Task<RefundDto> RequestRefundAsync(RequestRefundDto dto, string userId);
        Task<RefundDto> ProcessRefundAsync(int refundId, ProcessRefundDto dto);
        Task<IEnumerable<RefundDto>> GetAllRefundsAsync();
        Task<RefundDto?> GetRefundByIdAsync(int refundId);
        Task<IEnumerable<RefundDto>> GetRefundsByUserIdAsync(string userId);
    }
}
