using AngleSharp;
using Borealis.Core.Contracts;

namespace Borealis.Core.GiftCodeScanners;

public class DestructoidGiftCodeScanner : IGiftCodeScanner {
    private readonly HttpClient _httpClient;
    private readonly ILogger<DestructoidGiftCodeScanner> _logger;

    public string Name => "Destructoid";

    public DestructoidGiftCodeScanner(HttpClient httpClient, ILogger<DestructoidGiftCodeScanner> logger) {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ICollection<string>> ScanGiftCodesAsync(CancellationToken cancellationToken) {
        var pageSource = await _httpClient.GetStringAsync("https://www.destructoid.com/whiteout-survival-codes/", cancellationToken);

        if(string.IsNullOrWhiteSpace(pageSource)) {
            _logger.LogWarning("Failed to retrieve gift code page source.");

            return [];
        }

        var context = BrowsingContext.New();
        var document = await context.OpenAsync(req => req.Content(pageSource), cancellationToken);

        var giftCodeHeader = document.QuerySelector("#h-whiteout-survival-codes-working");
        if(giftCodeHeader == null) {
            _logger.LogWarning("Gift code header not found.");

            return [];
        }

        var giftCodeList = giftCodeHeader.NextElementSibling;
        if(giftCodeList == null || giftCodeList.TagName != "UL") {
            _logger.LogWarning("Gift code list not found.");

            return [];
        }

        var giftCodes = giftCodeList.QuerySelectorAll("li > strong:first-child").Select(x => x.TextContent).ToList();

        return giftCodes ?? [];
    }
}
