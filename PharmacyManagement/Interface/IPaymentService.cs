using PharmacyManagement.DTO;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface IPaymentService
    {
        Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(int orderId);
        Task<SaleDto> ConfirmPaymentAsync(ConfirmPaymentDto dto);
    }
}
