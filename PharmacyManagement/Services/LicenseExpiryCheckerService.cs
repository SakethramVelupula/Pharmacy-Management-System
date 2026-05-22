using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PharmacyManagement.Interface;

namespace PharmacyManagement.Services
{
    public class LicenseExpiryCheckerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LicenseExpiryCheckerService> _logger;
        private readonly TimeSpan _checkInterval;

        public LicenseExpiryCheckerService(IServiceScopeFactory scopeFactory, ILogger<LicenseExpiryCheckerService> logger, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            var hours = configuration.GetValue<int>("LicenseExpiry:CheckIntervalHours", 24);
            _checkInterval = TimeSpan.FromHours(hours);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LicenseExpiryCheckerService started. Interval: {Interval}h", _checkInterval.TotalHours);

            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckAndAlertAsync();
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task CheckAndAlertAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var expiringDoctors = (await authService.GetExpiringLicensesAsync()).ToList();

                if (!expiringDoctors.Any())
                {
                    _logger.LogInformation("LicenseExpiryCheckerService: No expiring licenses found.");
                    return;
                }

                _logger.LogWarning("LicenseExpiryCheckerService: Found {Count} expiring/expired licenses. Sending alert.", expiringDoctors.Count);

                await emailService.SendLicenseExpiryAlertAsync(
                    expiringDoctors.Select(d => (d.Name, d.Email, d.LicenseNumber, d.LicenseExpiryDate, d.DaysUntilExpiry))
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LicenseExpiryCheckerService: Error during license expiry check.");
            }
        }
    }
}
