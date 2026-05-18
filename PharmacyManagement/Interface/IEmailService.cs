namespace PharmacyManagement.Interface
{
    public interface IEmailService
    {
        Task SendOrderStatusEmailAsync(string doctorEmail, string doctorName, int orderId, string drugName, string status);
        Task SendWelcomeEmailAsync(string email, string name);
        Task SendNewOrderNotificationAsync(string doctorName, int orderId, string drugName, int quantity);
    }
}