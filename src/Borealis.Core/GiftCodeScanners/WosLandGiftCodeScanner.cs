using Borealis.Core.Contracts;
using Borealis.WhiteoutSurvivalHttpClient.Exceptions;

namespace Borealis.Core.GiftCodeScanners;

public class WosLandGiftCodeScanner : IGiftCodeScanner {
    private readonly IWosLandHttpClient _httpClient;
    private readonly ILogger<WosLandGiftCodeScanner> _logger;

    public string Name => "WOS Land";

    public WosLandGiftCodeScanner(IWosLandHttpClient httpClient, ILogger<WosLandGiftCodeScanner> logger) {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ICollection<string>> ScanGiftCodesAsync(CancellationToken cancellationToken) {
        try {
            return await _httpClient.GetGiftCodesAsync(cancellationToken);
        } catch(HttpUnauthorizedException ex) {
            _logger.LogError(ex, "Failed to scan gift codes from WOS Land. Most likely due to an invalid API Key.");
            return [];
        }
    }
}
