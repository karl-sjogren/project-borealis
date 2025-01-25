
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
        var scanners = scope.ServiceProvider.GetRequiredService<IEnumerable<IGiftCodeScanner>>();

        var giftCodes = new List<string>();
        try {
            foreach(var scanner in scanners) {
                var scannerGiftCodes = await scanner.ScanGiftCodesAsync(stoppingToken);
                giftCodes.AddRange(scannerGiftCodes);
            }

            if(giftCodes.Count == 0) {
                return;
            }

            var giftCodeService = scope.ServiceProvider.GetRequiredService<IGiftCodeService>();
            foreach(var giftCode in giftCodes) {
                _logger.LogInformation("Redeeming gift code {giftCode}.", giftCode);
                await giftCodeService.AddGiftCodeAsync(giftCode, stoppingToken);
            }
        } catch(Exception ex) {
            _logger.LogError(ex, "An error occurred while processing the gift code redemption queue.");
        }
    }
}
