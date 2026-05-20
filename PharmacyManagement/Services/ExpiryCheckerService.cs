using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PharmacyManagement.Interface;

namespace PharmacyManagement.Services
{
    public class ExpiryCheckerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpiryCheckerService> _logger;
        private readonly TimeSpan _checkInterval;

        public ExpiryCheckerService(IServiceScopeFactory scopeFactory, ILogger<ExpiryCheckerService> logger, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            var hours = configuration.GetValue<int>("DrugExpiry:CheckIntervalHours", 24);
            _checkInterval = TimeSpan.FromHours(hours);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExpiryCheckerService started. Interval: {Interval}h", _checkInterval.TotalHours);

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
                var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var expiredBatches = await inventoryService.GetExpiredBatchesAsync();
                var expiringBatches = await inventoryService.GetExpiringBatchesAsync();

                var allBatches = expiredBatches.Concat(expiringBatches).ToList();

                if (!allBatches.Any())
                {
                    _logger.LogInformation("ExpiryCheckerService: No expired or expiring batches found.");
                    return;
                }

                _logger.LogWarning("ExpiryCheckerService: Found {Count} expired/expiring batches. Sending alert.", allBatches.Count);

                await emailService.SendExpiryAlertAsync(
                    allBatches.Select(b => (b.DrugName, b.Quantity, b.ExpiryDate, b.DaysUntilExpiry, b.IsExpired))
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExpiryCheckerService: Error during expiry check.");
            }
        }
    }
}
