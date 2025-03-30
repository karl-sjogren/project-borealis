using System.ServiceModel.Syndication;
using System.Xml;
using Borealis.Core.Contracts;

namespace Borealis.Core.GiftCodeScanners;

public class WosGiftCodesGiftCodeScanner : IGiftCodeScanner {
    private readonly HttpClient _httpClient;
    private readonly ILogger<WosGiftCodesGiftCodeScanner> _logger;

    public string Name => "wosgiftcodes.com";

    public WosGiftCodesGiftCodeScanner(HttpClient httpClient, ILogger<WosGiftCodesGiftCodeScanner> logger) {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ICollection<string>> ScanGiftCodesAsync(CancellationToken cancellationToken) {
        string rssSource;
        try {
            rssSource = await _httpClient.GetStringAsync("https://wosgiftcodes.com/rss.php", cancellationToken);
        } catch(HttpRequestException ex) {
            _logger.LogError(ex, "Failed to retrieve gift code rss source.");

            return [];
        }

        if(string.IsNullOrWhiteSpace(rssSource)) {
            _logger.LogError("Gift code rss source is empty.");

            return [];
        }

        try {
            using var stringReader = new StringReader(rssSource);
            using var xmlReader = XmlReader.Create(stringReader);
            var feed = SyndicationFeed.Load(xmlReader);

            return feed.Items.Select(item => item.Title.Text).ToList();
        } catch(Exception ex) {
            _logger.LogError(ex, "Failed to parse gift code rss source.");

            return [];
        }
    }
}
