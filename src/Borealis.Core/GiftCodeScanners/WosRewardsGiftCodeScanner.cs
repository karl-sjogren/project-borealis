using AngleSharp;
using Borealis.Core.Contracts;

namespace Borealis.Core.GiftCodeScanners;

public class WosRewardsGiftCodeScanner : IGiftCodeScanner {
    private readonly HttpClient _httpClient;
    private readonly ILogger<WosRewardsGiftCodeScanner> _logger;

    public WosRewardsGiftCodeScanner(HttpClient httpClient, ILogger<WosRewardsGiftCodeScanner> logger) {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ICollection<string>> ScanGiftCodesAsync(CancellationToken cancellationToken) {
        var pageSource = await _httpClient.GetStringAsync("https://wosrewards.com/", cancellationToken);

        if(string.IsNullOrWhiteSpace(pageSource)) {
            _logger.LogWarning("Failed to retrieve gift code page source.");

            return [];
        }

        var context = BrowsingContext.New();
        var document = await context.OpenAsync(req => req.Content(pageSource), cancellationToken);

        var giftCodeHeader = document.QuerySelector("h3:contains('Gift Codes')");
        if(giftCodeHeader == null) {
            _logger.LogWarning("Gift code header not found.");

            return [];
        }

        var giftCodeContainer = giftCodeHeader.NextElementSibling;
        if(giftCodeContainer == null || giftCodeContainer.TagName != "DIV") {
            _logger.LogWarning("Gift code container not found.");

            return [];
        }

        var giftCodes = giftCodeContainer.QuerySelectorAll("h5").Select(x => x.TextContent).ToList();

        return giftCodes ?? [];
    }
}
