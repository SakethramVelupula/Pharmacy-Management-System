namespace PharmacyManagement.Interface
{
    public interface IEmailService
    {
        Task SendOrderStatusEmailAsync(string doctorEmail, string doctorName, int orderId, string drugName, string status);
        Task SendWelcomeEmailAsync(string email, string name);
        Task SendNewOrderNotificationAsync(string doctorName, int orderId, string drugName, int quantity);
        Task SendDoctorPendingApprovalAsync(string email, string name);
        Task SendAdminNewDoctorRegistrationAsync(string doctorName, string doctorEmail, string clinicName, string licenseNumber);
        Task SendDoctorApprovedAsync(string email, string name);
        Task SendDoctorRejectedAsync(string email, string name, string reason);
        Task SendExpiryAlertAsync(IEnumerable<(string DrugName, int Quantity, DateTime ExpiryDate, int DaysUntilExpiry, bool IsExpired)> batches);
    }
}