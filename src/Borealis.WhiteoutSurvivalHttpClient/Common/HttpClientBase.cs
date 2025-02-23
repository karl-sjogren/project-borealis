using System.Net;
using System.Text.Json;
using Borealis.WhiteoutSurvivalHttpClient.Exceptions;

namespace Borealis.WhiteoutSurvivalHttpClient.Common;

public abstract class HttpClientBase {
    private readonly ILogger _logger;

    protected HttpClientBase(HttpClient httpClient, ILogger logger) {
        HttpClient = httpClient;
        _logger = logger;
    }

    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    protected virtual JsonSerializerOptions SerializerOptions => _serializerOptions;

    protected HttpClient HttpClient { get; }

    protected virtual async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response, CancellationToken cancellationToken) {
        if(response.IsSuccessStatusCode) {
            return;
        }

        var errorMessage = await GetErrorMessageAsync(response, cancellationToken);

        if(response.StatusCode == HttpStatusCode.BadRequest) {
            throw new HttpBadRequestException(errorMessage, $"400 Bad Request was returned for call to {response.RequestMessage?.RequestUri}.");
        }

        if(response.StatusCode == HttpStatusCode.Unauthorized) {
            throw new HttpUnauthorizedException(errorMessage, $"401 Unauthorized was returned for call to {response.RequestMessage?.RequestUri}.");
        }

        if(response.StatusCode == HttpStatusCode.NotFound) {
            throw new HttpNotFoundException(errorMessage, $"404 Not Found was returned for call to {response.RequestMessage?.RequestUri}.");
        }

        if(response.StatusCode == HttpStatusCode.InternalServerError) {
            throw new HttpInternalServerErrorException(errorMessage, $"500 Internal Server Error was returned for call to {response.RequestMessage?.RequestUri}.");
        }

        if(response.StatusCode == HttpStatusCode.ServiceUnavailable) {
            throw new HttpServiceUnavailableException(errorMessage, $"503 Service Unavailable was returned for call to {response.RequestMessage?.RequestUri}.");
        }

        throw new HttpStatusCodeException(response.StatusCode, errorMessage, $"A non-successful status code ({response.StatusCode}) was returned for call to {response.RequestMessage?.RequestUri}.");
    }

    protected virtual async Task<string> GetErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken) {
        try {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return content ?? string.Empty;
        } catch {
            return string.Empty;
        }
    }

    protected virtual async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken) {
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        try {
            return await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions, cancellationToken);
        } catch(Exception exception) {
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(exception, "Failed to deserialize JSON response: {Json}", json);
            return default;
        }
    }
}
