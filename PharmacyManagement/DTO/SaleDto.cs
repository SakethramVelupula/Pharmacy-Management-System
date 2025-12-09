using System;
using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.DTO
{

    public class SaleDto
    {
        public int SalesId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int DrugId { get; set; }
        public string DrugName { get; set; }
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; }
    }

    
    public class CreateSaleDto
    {
        [Required]
        public int OrderId { get; set; }
        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Cash";
    }

    
    public class SalesAnalyticsDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalSales { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<TopSellingDrugDto> TopSellingDrugs { get; set; }
        public List<DailySalesDto> DailySales { get; set; }
        public List<PaymentMethodDto> PaymentMethods { get; set; }
    }

    public class TopSellingDrugDto
    {
        public string DrugName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DailySalesDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class PaymentMethodDto
    {
        public string Method { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }
}