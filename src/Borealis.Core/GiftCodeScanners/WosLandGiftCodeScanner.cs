using System.Net.Http.Json;
using Borealis.Core.Contracts;

namespace Borealis.Core.GiftCodeScanners;

public class WosLandGiftCodeScanner : IGiftCodeScanner {
    private readonly HttpClient _httpClient;
    private readonly ILogger<WosRewardsGiftCodeScanner> _logger;

    public string Name => "WOS Land";

    public WosLandGiftCodeScanner(HttpClient httpClient, ILogger<WosRewardsGiftCodeScanner> logger) {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ICollection<string>> ScanGiftCodesAsync(CancellationToken cancellationToken) {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://wosland.com/apidc/giftapi/giftcode_api.php");
        request.Headers.Add("X-API-Key", "serioyun_gift_api_key_2024");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseDTO = await response.Content.ReadFromJsonAsync<ResponseDTO>(cancellationToken);

        var giftCodes = responseDTO?.codes.Select(x => x.Substring(0, x.IndexOf(" ", StringComparison.OrdinalIgnoreCase))).ToList();

        return giftCodes ?? [];
    }
}

file record ResponseDTO(string[] codes);
