
using PharmacyManagement.DTO;

namespace PharmacyManagement.Interface
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(string userId);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string userId);
        Task<OrderDto> CreatePatientOrderAsync(CreatePatientOrderDto dto, string userId);
        Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderDto dto);
        Task<bool> DeleteOrderAsync(int id);
    }
}
