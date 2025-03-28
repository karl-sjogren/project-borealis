using AngleSharp;
using Borealis.Core.Contracts;

namespace Borealis.Core.GiftCodeScanners;

public class WosRewardsGiftCodeScanner : IGiftCodeScanner {
    private readonly HttpClient _httpClient;
    private readonly ILogger<WosRewardsGiftCodeScanner> _logger;

    public string Name => "WOS Rewards";

    public WosRewardsGiftCodeScanner(HttpClient httpClient, ILogger<WosRewardsGiftCodeScanner> logger) {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ICollection<string>> ScanGiftCodesAsync(CancellationToken cancellationToken) {
        var pageSource = await _httpClient.GetStringAsync("https://wosrewards.com/", cancellationToken);

        if(string.IsNullOrWhiteSpace(pageSource)) {
            _logger.LogError("Failed to retrieve gift code page source.");

            return [];
        }

        var context = BrowsingContext.New();
        var document = await context.OpenAsync(req => req.Content(pageSource), cancellationToken);

        var giftCodeHeader = document.QuerySelector("h3:contains('Available Gift Codes')");
        if(giftCodeHeader == null) {
            _logger.LogError("Gift code header not found.");

            return [];
        }

        var giftCodeContainer = giftCodeHeader.NextElementSibling;
        if(giftCodeContainer == null || giftCodeContainer.TagName != "DIV") {
            _logger.LogError("Gift code container not found.");

            return [];
        }

        var giftCodeElements = giftCodeContainer.QuerySelectorAll("h5");
        if(giftCodeElements.Length == 0) {
            _logger.LogWarning("No gift codes found.");

            return [];
        }

        var giftCodes = giftCodeElements.Select(x => x.TextContent).ToList();

        return giftCodes ?? [];
    }
}
