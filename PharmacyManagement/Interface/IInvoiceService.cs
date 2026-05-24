namespace PharmacyManagement.Interface
{
    public interface IInvoiceService
    {
        Task<byte[]> GenerateInvoiceAsync(int orderId);
    }
}
