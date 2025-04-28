using System.Net.Http.Json;
using Borealis.Core.Contracts;
using Borealis.Core.HttpClients.DataTransferObjects;
using Borealis.Core.Options;
using Microsoft.Extensions.Options;

namespace Borealis.Core.HttpClients;

public class CapSolverHttpClient : HttpClientBase, ICapSolverHttpClient {
    private readonly CapSolverOptions _options;

    public CapSolverHttpClient(HttpClient httpClient, IOptions<CapSolverOptions> options, ILogger logger) : base(httpClient, logger) {
        _options = options.Value;
    }

    public async Task<CapSolverImageToTextResponse?> ImageToTextAsync(byte[] buffer, CancellationToken cancellationToken) {
        var requestObject = new CapSolverImageToTextRequest {
            ClientKey = _options.ClientKey,
            Task = new CapSolverImageToTextTask {
                Body = Convert.ToBase64String(buffer)
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "createTask") {
            Content = JsonContent.Create(requestObject)
        };

        var response = await HttpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessStatusCodeAsync(response, cancellationToken);

        var result = await DeserializeResponseAsync<CapSolverImageToTextResponse>(response, cancellationToken);

        return result!;
    }
}
