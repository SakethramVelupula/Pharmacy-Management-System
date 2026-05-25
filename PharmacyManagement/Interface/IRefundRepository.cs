using PharmacyManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface IRefundRepository
    {
        Task<Refund> CreateAsync(Refund refund);
        Task<Refund?> GetByIdAsync(int refundId);
        Task<Refund?> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<Refund>> GetAllAsync();
        Task<IEnumerable<Refund>> GetByUserIdAsync(string userId);
        Task<Refund?> UpdateAsync(Refund refund);
    }
}
