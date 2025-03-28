
using System.Net.Http.Json;
using Borealis.Core.Contracts;

namespace Borealis.Core.HttpClients;

public class WosLandHttpClient : HttpClientBase, IWosLandHttpClient {
    private readonly ILogger<WosLandHttpClient> _logger;

    public WosLandHttpClient(HttpClient httpClient, ILogger<WosLandHttpClient> logger) : base(httpClient, logger) {
        _logger = logger;
    }

    public async Task<ICollection<string>> GetGiftCodesAsync(CancellationToken cancellationToken) {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://wosland.com/apidc/giftapi/giftcode_api.php");

        var response = await HttpClient.SendAsync(request, cancellationToken);
        var responseDTO = await response.Content.ReadFromJsonAsync<ResponseDTO>(cancellationToken);

        await EnsureSuccessStatusCodeAsync(response, cancellationToken);

        var giftCodes = responseDTO?.codes.Select(code => code.Substring(0, code.IndexOf(" ", StringComparison.OrdinalIgnoreCase))).ToList();

        return giftCodes ?? [];
    }
}

file record ResponseDTO(string[] codes);
