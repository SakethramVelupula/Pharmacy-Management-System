using System.Net;
using System.Net.Mail;
using PharmacyManagement.Interface;

namespace PharmacyManagement.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendOrderStatusEmailAsync(string doctorEmail, string doctorName, int orderId, string drugName, string status)
        {
            var subject = $"Order #{orderId} Status Update - {status}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #667eea; padding: 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>Pharmacy Management System</h1>
                    </div>
                    <div style='padding: 30px; background-color: #f9f9f9;'>
                        <h2>Order Status Update</h2>
                        <p>Dear Dr. {doctorName},</p>
                        <p>Your order has been updated. Here are the details:</p>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr style='background-color: #667eea; color: white;'>
                                <th style='padding: 10px; text-align: left;'>Field</th>
                                <th style='padding: 10px; text-align: left;'>Details</th>
                            </tr>
                            <tr style='background-color: white;'>
                                <td style='padding: 10px; border: 1px solid #ddd;'>Order ID</td>
                                <td style='padding: 10px; border: 1px solid #ddd;'>#{orderId}</td>
                            </tr>
                            <tr style='background-color: #f2f2f2;'>
                                <td style='padding: 10px; border: 1px solid #ddd;'>Drug</td>
                                <td style='padding: 10px; border: 1px solid #ddd;'>{drugName}</td>
                            </tr>
                            <tr style='background-color: white;'>
                                <td style='padding: 10px; border: 1px solid #ddd;'>Status</td>
                                <td style='padding: 10px; border: 1px solid #ddd;'>
                                    <span style='background-color: {GetStatusColor(status)}; color: white; padding: 4px 10px; border-radius: 12px;'>{status}</span>
                                </td>
                            </tr>
                        </table>
                        <p style='margin-top: 20px;'>If you have any questions, please contact the pharmacy.</p>
                    </div>
                    <div style='background-color: #333; padding: 15px; text-align: center;'>
                        <p style='color: #aaa; margin: 0; font-size: 12px;'>Pharmacy Management System - Automated Notification</p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(doctorEmail, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string email, string name)
        {
            var subject = "Welcome to Pharmacy Management System";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #667eea; padding: 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>Pharmacy Management System</h1>
                    </div>
                    <div style='padding: 30px; background-color: #f9f9f9;'>
                        <h2>Welcome, Dr. {name}!</h2>
                        <p>Your account has been successfully created.</p>
                        <p>You can now:</p>
                        <ul>
                            <li>Browse available drugs and check stock</li>
                            <li>Place prescription orders</li>
                            <li>Request special orders for unavailable drugs</li>
                            <li>Track your order status in real time</li>
                        </ul>
                        <p>Login at: <a href='http://localhost:4200'>Pharmacy Management System</a></p>
                    </div>
                    <div style='background-color: #333; padding: 15px; text-align: center;'>
                        <p style='color: #aaa; margin: 0; font-size: 12px;'>Pharmacy Management System - Automated Notification</p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendNewOrderNotificationAsync(string doctorName, int orderId, string drugName, int quantity)
        {
            var adminEmail = _configuration["Email:SenderEmail"];
            var subject = $"New Order #{orderId} Received";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #667eea; padding: 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>Pharmacy Management System</h1>
                    </div>
                    <div style='padding: 30px; background-color: #f9f9f9;'>
                        <h2>New Order Received</h2>
                        <p>A new prescription order has been placed.</p>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr style='background-color: #667eea; color: white;'>
                                <th style='padding: 10px; text-align: left;'>Field</th>
                                <th style='padding: 10px; text-align: left;'>Details</th>
                            </tr>
                            <tr style='background-color: white;'>
                                <td style='padding: 10px; border: 1px solid #ddd;'>Order ID</td>
                                <td style='padding: 10px; border: 1px solid #ddd;'>#{orderId}</td>
                            </tr>
                            <tr style='background-color: #f2f2f2;'>
                                <td style='padding: 10px; border: 1px solid #ddd;'>Doctor</td>
                                <td style='padding: 10px; border: 1px solid #ddd;'>Dr. {doctorName}</td>
                            </tr>
                            <tr style='background-color: white;'>
                                <td style='padding: 10px; border: 1px solid #ddd;'>Drug</td>
                                <td style='padding: 10px; border: 1px solid #ddd;'>{drugName}</td>
                            </tr>
                            <tr style='background-color: #f2f2f2;'>
                                <td style='padding: 10px; border: 1px solid #ddd;'>Quantity</td>
                                <td style='padding: 10px; border: 1px solid #ddd;'>{quantity}</td>
                            </tr>
                        </table>
                        <p style='margin-top: 20px;'>Please review and process this order.</p>
                    </div>
                    <div style='background-color: #333; padding: 15px; text-align: center;'>
                        <p style='color: #aaa; margin: 0; font-size: 12px;'>Pharmacy Management System - Automated Notification</p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(adminEmail!, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"]!);
                var senderEmail = _configuration["Email:SenderEmail"];
                var senderName = _configuration["Email:SenderName"];
                var password = _configuration["Email:Password"];

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail!, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            }
        }

        public async Task SendDoctorPendingApprovalAsync(string email, string name)
        {
            var subject = "Your Doctor Account is Pending Approval";
            var body = $@"
                <html><body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #667eea; padding: 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>Pharmacy Management System</h1>
                    </div>
                    <div style='padding: 30px; background-color: #f9f9f9;'>
                        <h2>Registration Received, Dr. {name}!</h2>
                        <p>Your doctor account registration has been received and is currently <strong>pending admin approval</strong>.</p>
                        <p>You will receive an email once your account has been reviewed.</p>
                        <div style='background-color: #fff3cd; padding: 15px; border-radius: 6px; border-left: 4px solid #ffc107;'>
                            <p style='margin: 0;'>⏳ <strong>Status: Pending Approval</strong></p>
                        </div>
                    </div>
                    <div style='background-color: #333; padding: 15px; text-align: center;'>
                        <p style='color: #aaa; margin: 0; font-size: 12px;'>Pharmacy Management System - Automated Notification</p>
                    </div>
                </body></html>";
            await SendEmailAsync(email, subject, body);
        }

        public async Task SendAdminNewDoctorRegistrationAsync(string doctorName, string doctorEmail, string clinicName, string licenseNumber)
        {
            var adminEmail = _configuration["Email:SenderEmail"];
            var subject = $"New Doctor Registration Pending Approval - Dr. {doctorName}";
            var body = $@"
                <html><body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #667eea; padding: 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>Pharmacy Management System</h1>
                    </div>
                    <div style='padding: 30px; background-color: #f9f9f9;'>
                        <h2>New Doctor Registration</h2>
                        <p>A new doctor has registered and requires your approval:</p>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr style='background-color: #667eea; color: white;'>
                                <th style='padding: 10px; text-align: left;'>Field</th>
                                <th style='padding: 10px; text-align: left;'>Details</th>
                            </tr>
                            <tr style='background-color: white;'>
                                <td style='padding: 10px; border: 1px solid #ddd;'>Name</td>
                                <td style='padding: 10px; border: 1px solid #ddd;'>Dr. {doctorName}</td>
                            </tr>
                            <tr style='background-color: #f2f2f2;'>
                                <td style='padding: 10px; border: 1px solid #ddd;'>Email</td>
                                <td style='padding: 10px; border: 1px solid #ddd;'>{doctorEmail}</td>
                            </tr>
                            <tr style='background-color: white;'>
                                <td style='padding: 10px; border: 1px solid #ddd;'>Clinic</td>
                                <td style='padding: 10px; border: 1px solid #ddd;'>{clinicName}</td>
                            </tr>
                            <tr style='background-color: #f2f2f2;'>
                                <td style='padding: 10px; border: 1px solid #ddd;'>License Number</td>
                                <td style='padding: 10px; border: 1px solid #ddd;'>{licenseNumber}</td>
                            </tr>
                        </table>
                        <p style='margin-top: 20px;'>Please login to the admin panel to approve or reject this registration.</p>
                    </div>
                    <div style='background-color: #333; padding: 15px; text-align: center;'>
                        <p style='color: #aaa; margin: 0; font-size: 12px;'>Pharmacy Management System - Automated Notification</p>
                    </div>
                </body></html>";
            await SendEmailAsync(adminEmail!, subject, body);
        }

        public async Task SendDoctorApprovedAsync(string email, string name)
        {
            var subject = "Your Doctor Account Has Been Approved!";
            var body = $@"
                <html><body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #667eea; padding: 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>Pharmacy Management System</h1>
                    </div>
                    <div style='padding: 30px; background-color: #f9f9f9;'>
                        <h2>Account Approved, Dr. {name}!</h2>
                        <div style='background-color: #d4edda; padding: 15px; border-radius: 6px; border-left: 4px solid #28a745;'>
                            <p style='margin: 0;'>✅ <strong>Your account has been approved!</strong></p>
                        </div>
                        <p style='margin-top: 20px;'>You can now login and start placing orders.</p>
                        <p>Login at: <a href='http://localhost:4200/login'>Pharmacy Management System</a></p>
                    </div>
                    <div style='background-color: #333; padding: 15px; text-align: center;'>
                        <p style='color: #aaa; margin: 0; font-size: 12px;'>Pharmacy Management System - Automated Notification</p>
                    </div>
                </body></html>";
            await SendEmailAsync(email, subject, body);
        }

        public async Task SendDoctorRejectedAsync(string email, string name, string reason)
        {
            var subject = "Your Doctor Account Registration Was Not Approved";
            var body = $@"
                <html><body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #667eea; padding: 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>Pharmacy Management System</h1>
                    </div>
                    <div style='padding: 30px; background-color: #f9f9f9;'>
                        <h2>Registration Not Approved, Dr. {name}</h2>
                        <div style='background-color: #f8d7da; padding: 15px; border-radius: 6px; border-left: 4px solid #dc3545;'>
                            <p style='margin: 0;'>❌ <strong>Your registration was not approved.</strong></p>
                        </div>
                        <p style='margin-top: 20px;'><strong>Reason:</strong> {reason}</p>
                        <p>If you believe this is a mistake, please contact the pharmacy directly.</p>
                    </div>
                    <div style='background-color: #333; padding: 15px; text-align: center;'>
                        <p style='color: #aaa; margin: 0; font-size: 12px;'>Pharmacy Management System - Automated Notification</p>
                    </div>
                </body></html>";
            await SendEmailAsync(email, subject, body);
        }

        private static string GetStatusColor(string status) => status switch
        {
            "Delivered" => "#28a745",
            "Processing" => "#ffc107",
            "Cancelled" => "#dc3545",
            _ => "#667eea"
        };
    }
}