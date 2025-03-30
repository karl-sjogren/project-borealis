
using Borealis.Core.Contracts;

namespace Borealis.Web.HostedServices;

public class ScanForGiftCodesHostedService : TimedHostedService {
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ScanForGiftCodesHostedService> _logger;

    public ScanForGiftCodesHostedService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ScanForGiftCodesHostedService> logger)
            : base(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(5), logger) {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        using var scope = _serviceScopeFactory.CreateScope();
        var giftCodeService = scope.ServiceProvider.GetRequiredService<IGiftCodeService>();
        var scanners = scope.ServiceProvider.GetRequiredService<IEnumerable<IGiftCodeScanner>>();

        try {
            foreach(var scanner in scanners) {
                var scannerGiftCodes = await scanner.ScanGiftCodesAsync(stoppingToken);

                foreach(var giftCode in scannerGiftCodes) {
                    var existsResult = await giftCodeService.GiftCodeExistsAsync(giftCode, stoppingToken);
                    if(existsResult.Data) {
                        continue;
                    }

                    _logger.LogInformation("Trying to add gift code {giftCode}.", giftCode);
                    await giftCodeService.AddGiftCodeAsync(giftCode, scanner.Name, stoppingToken);
                }
            }
        } catch(Exception ex) {
            _logger.LogError(ex, "An error occurred while processing the gift code scanner queue.");
        }
    }
}
