using PharmacyManagement.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyManagement.Interface
{
    public interface ISalesService
    {
        Task<IEnumerable<SaleDto>> GetAllSalesAsync();
        Task<SaleDto> GetSaleByIdAsync(int id);
        Task<SaleDto> CreateSaleFromOrderAsync(CreateSaleDto dto);
        Task<SalesAnalyticsDto> GetSalesAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}