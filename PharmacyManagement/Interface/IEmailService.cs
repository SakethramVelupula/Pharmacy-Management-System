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
        Task SendLowStockAlertAsync(IEnumerable<(string DrugName, int CurrentStock, int Threshold)> drugs);
        Task SendLicenseExpiryAlertAsync(IEnumerable<(string DoctorName, string Email, string LicenseNumber, DateTime ExpiryDate, int DaysUntilExpiry)> doctors);
        Task SendInvoiceEmailAsync(string toEmail, string name, int orderId, byte[] pdfBytes);
        Task SendRefundRequestedAsync(string adminEmail, string requesterName, int orderId, string reason, string refundType);
        Task SendRefundProcessedAsync(string toEmail, string name, int orderId, bool isApproved, decimal amount, string refundMethod, string? adminNotes);
        Task SendPasswordResetEmailAsync(string toEmail, string name, string resetToken);
    }
}