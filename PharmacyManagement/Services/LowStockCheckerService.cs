using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PharmacyManagement.Interface;

namespace PharmacyManagement.Services
{
    public class LowStockCheckerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LowStockCheckerService> _logger;
        private readonly TimeSpan _checkInterval;

        public LowStockCheckerService(IServiceScopeFactory scopeFactory, ILogger<LowStockCheckerService> logger, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            var hours = configuration.GetValue<int>("LowStock:CheckIntervalHours", 24);
            _checkInterval = TimeSpan.FromHours(hours);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LowStockCheckerService started. Interval: {Interval}h", _checkInterval.TotalHours);

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
                var drugService = scope.ServiceProvider.GetRequiredService<IDrugsService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var lowStockDrugs = (await drugService.GetLowStockDrugsAsync()).ToList();

                if (!lowStockDrugs.Any())
                {
                    _logger.LogInformation("LowStockCheckerService: No low stock drugs found.");
                    return;
                }

                _logger.LogWarning("LowStockCheckerService: Found {Count} low stock drugs. Sending alert.", lowStockDrugs.Count);

                await emailService.SendLowStockAlertAsync(
                    lowStockDrugs.Select(d => (d.Name, d.Stock, d.LowStockThreshold))
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LowStockCheckerService: Error during low stock check.");
            }
        }
    }
}
