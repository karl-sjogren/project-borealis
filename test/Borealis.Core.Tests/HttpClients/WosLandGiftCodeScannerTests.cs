using System.Net;
using Borealis.Core.HttpClients;
using Borealis.TestHelpers;
using Borealis.TestHelpers.Http;
using Borealis.WhiteoutSurvivalHttpClient.Exceptions;

namespace Borealis.Core.Tests.HttpClients;

public class WosLandHttpClientTests {
    private static WosLandHttpClient CreateClient(HttpClient httpClient) => new(httpClient, NullLogger<WosLandHttpClient>.Instance);

    [Fact]
    public async Task GetGiftCodesSuccessAsync() {
        var httpClient = HttpClientActivator<WosLandHttpClient>.GetClient(async (request, _) => {
            if(request.RequestUri?.PathAndQuery.EndsWith("/giftcode_api.php", StringComparison.Ordinal) == true) {
                var json = await Resources.GetStringAsync("WosLandSuccess.json");
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new FakeHttpContent(json) };
            }

            throw new Exception("Unexpected uri was called during test. " + request.RequestUri?.PathAndQuery);
        }, CreateClient);

        var result = await httpClient.GetGiftCodesAsync(TestCancellationToken);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.ShouldContain("code-1");
        result.ShouldContain("code-2");
    }

    [Fact]
    public async Task GetGiftCodesWrongApiKeyAsync() {
        var httpClient = HttpClientActivator<WosLandHttpClient>.GetClient(async (request, _) => {
            if(request.RequestUri?.PathAndQuery.EndsWith("/giftcode_api.php", StringComparison.Ordinal) == true) {
                var json = await Resources.GetStringAsync("WosLandWrongApiKey.json");
                return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new FakeHttpContent(json) };
            }

            throw new Exception("Unexpected uri was called during test. " + request.RequestUri?.PathAndQuery);
        }, CreateClient);

        await Should.ThrowAsync<HttpUnauthorizedException>(async () => await httpClient.GetGiftCodesAsync(TestCancellationToken));
    }
}
