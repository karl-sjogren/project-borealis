using System.Net.Http.Json;
using Borealis.Core.Contracts;
using Borealis.WhiteoutSurvivalHttpClient.Models;

namespace Borealis.Core.HttpClients;

public class WhiteoutSurvivalHttpClientProxy : HttpClientBase, IWhiteoutSurvivalHttpClientProxy {
    public WhiteoutSurvivalHttpClientProxy(HttpClient httpClient, ILogger<WhiteoutSurvivalHttpClientProxy> logger) : base(httpClient, logger) {
    }

    public async Task<WhiteoutSurvivalResponseWrapper<WhiteoutSurvivalPlayerResponse>> GetPlayerInfoAsync(int playerId, CancellationToken cancellationToken) {
        var request = new HttpRequestMessage(HttpMethod.Get, $"api/GetPlayerInfo?playerId={playerId}");

        var response = await HttpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessStatusCodeAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<WhiteoutSurvivalResponseWrapper<WhiteoutSurvivalPlayerResponse>>(cancellationToken);

        return result!;
    }
}
