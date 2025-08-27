
using System.Net.Http.Json;
using Borealis.Core.Contracts;

namespace Borealis.Core.HttpClients;

public class WhiteoutBotHttpClient : HttpClientBase, IWhiteoutBotHttpClient {
    private readonly ILogger<WhiteoutBotHttpClient> _logger;

    public WhiteoutBotHttpClient(HttpClient httpClient, ILogger<WhiteoutBotHttpClient> logger) : base(httpClient, logger) {
        _logger = logger;
    }

    public async Task<ICollection<string>> GetGiftCodesAsync(CancellationToken cancellationToken) {
        var request = new HttpRequestMessage(HttpMethod.Get, "giftcode_api.php");

        var response = await HttpClient.SendAsync(request, cancellationToken);
        var responseDTO = await response.Content.ReadFromJsonAsync<ResponseDTO>(cancellationToken);

        await EnsureSuccessStatusCodeAsync(response, cancellationToken);

        var giftCodes = responseDTO?.codes.Select(code => code[..code.IndexOf(" ", StringComparison.OrdinalIgnoreCase)]).ToList();

        return giftCodes ?? [];
    }
}

file record ResponseDTO(string[] codes);
