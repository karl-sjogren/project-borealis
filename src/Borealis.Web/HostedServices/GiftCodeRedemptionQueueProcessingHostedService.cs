
using Borealis.Core.Contracts;

namespace Borealis.Web.HostedServices;

public class GiftCodeRedemptionQueueProcessingHostedService : BackgroundService {
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<GiftCodeRedemptionQueueProcessingHostedService> _logger;

    public GiftCodeRedemptionQueueProcessingHostedService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<GiftCodeRedemptionQueueProcessingHostedService> logger) {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while(!stoppingToken.IsCancellationRequested) {
            using var scope = _serviceScopeFactory.CreateScope();
            var queue = scope.ServiceProvider.GetRequiredService<IGiftCodeRedemptionQueue>();

            try {
                await queue.ProcessQueueAsync(stoppingToken);
            } catch(Exception ex) {
                _logger.LogError(ex, "An error occurred while processing the gift code redemption queue.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
