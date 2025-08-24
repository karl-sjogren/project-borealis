using Borealis.Core.Contracts;
using Borealis.WhiteoutSurvivalHttpClient.Exceptions;

namespace Borealis.Core.GiftCodeScanners;

public class WhiteoutBotGiftCodeScanner : IGiftCodeScanner {
    private readonly IWhiteoutBotHttpClient _httpClient;
    private readonly ILogger<WhiteoutBotGiftCodeScanner> _logger;

    public string Name => "Whiteout Bot";

    public WhiteoutBotGiftCodeScanner(IWhiteoutBotHttpClient httpClient, ILogger<WhiteoutBotGiftCodeScanner> logger) {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ICollection<string>> ScanGiftCodesAsync(CancellationToken cancellationToken) {
        try {
            return await _httpClient.GetGiftCodesAsync(cancellationToken);
        } catch(HttpForbiddenException ex) {
            _logger.LogError(ex, "Failed to scan gift codes from Whiteout Bot. Most likely due to an invalid API Key.");
            return [];
        }
    }
}
