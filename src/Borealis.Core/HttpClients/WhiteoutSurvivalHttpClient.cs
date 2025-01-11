using System.Globalization;
using System.Text.Json;
using Borealis.Core.Common;
using Borealis.Core.Contracts;
using Borealis.Core.HttpClients.Models;
using Borealis.Core.Options;
using Microsoft.Extensions.Options;

namespace Borealis.Core.HttpClients;

public class WhiteoutSurvivalHttpClient : HttpClientBase, IWhiteoutSurvivalHttpClient {
    private readonly HttpClient _httpClient;
    private readonly WhiteoutSurvivalOptions _options;
    private readonly TimeProvider _timeProvider;

    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    protected override JsonSerializerOptions SerializerOptions => _serializerOptions;

    public WhiteoutSurvivalHttpClient(HttpClient httpClient, IOptions<WhiteoutSurvivalOptions> options, TimeProvider timeProvider) : base(httpClient) {
        _httpClient = httpClient;
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public async Task<WhiteoutSurvivalResponseWrapper<WhiteoutSurvivalPlayerResponse>> GetPlayerInfoAsync(int playerId, CancellationToken cancellationToken) {
        var data = new Dictionary<string, string>() {
            { "fid", playerId.ToString(CultureInfo.InvariantCulture) },
            { "time", _timeProvider.GetUtcNow().ToUnixTimeSeconds().ToString() }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "player") {
            Content = new WhiteoutSurvivalEncodedRequestContent(_options.Secret, data)
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessStatusCodeAsync(response, cancellationToken);

        var result = await DeserializeResponseAsync<WhiteoutSurvivalResponseWrapper<WhiteoutSurvivalPlayerResponse>>(response, cancellationToken);

        return result!;
    }

    public async Task<WhiteoutSurvivalResponseWrapper> RedeemGiftCodeAsync(int playerId, string giftCode, CancellationToken cancellationToken) {
        var data = new Dictionary<string, string>() {
            { "fid", playerId.ToString(CultureInfo.InvariantCulture) },
            { "cdk", giftCode },
            { "time", _timeProvider.GetUtcNow().ToUnixTimeSeconds().ToString() }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "gift_code") {
            Content = new WhiteoutSurvivalEncodedRequestContent(_options.Secret, data)
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessStatusCodeAsync(response, cancellationToken);

        var result = await DeserializeResponseAsync<WhiteoutSurvivalResponseWrapper>(response, cancellationToken);

        return result!;
    }
}
