using PharmacyManagement.Models;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface IPaymentRepository
    {
        Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction);
        Task<PaymentTransaction?> GetByPaymentIntentIdAsync(string paymentIntentId);
        Task<PaymentTransaction?> UpdateStatusAsync(string paymentIntentId, string status);
    }
}
