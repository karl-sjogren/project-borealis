using System.Net;
using Borealis.Core.HttpClients;
using Borealis.TestHelpers;
using Borealis.TestHelpers.Http;
using Borealis.WhiteoutSurvivalHttpClient.Exceptions;

namespace Borealis.Core.Tests.HttpClients;

public class WhiteoutBotHttpClientTests {
    private static WhiteoutBotHttpClient CreateClient(HttpClient httpClient) => new(httpClient, NullLogger<WhiteoutBotHttpClient>.Instance);

    [Fact]
    public async Task GetGiftCodesSuccessAsync() {
        var httpClient = HttpClientActivator<WhiteoutBotHttpClient>.GetClient(async (request, _) => {
            if(request.RequestUri?.PathAndQuery.EndsWith("/giftcode_api.php", StringComparison.Ordinal) == true) {
                var json = await Resources.GetStringAsync("WhiteoutBotSuccess.json");
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new FakeHttpContent(json) };
            }

            throw new Exception("Unexpected uri was called during test. " + request.RequestUri?.PathAndQuery);
        }, CreateClient);

        var result = await httpClient.GetGiftCodesAsync(TestCancellationToken);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(5);
        result.ShouldContain("code-1");
        result.ShouldContain("code-2");
        result.ShouldContain("code-3");
        result.ShouldContain("code-4");
        result.ShouldContain("code-5");
    }

    [Fact]
    public async Task GetGiftCodesWrongApiKeyAsync() {
        var httpClient = HttpClientActivator<WhiteoutBotHttpClient>.GetClient(async (request, _) => {
            if(request.RequestUri?.PathAndQuery.EndsWith("/giftcode_api.php", StringComparison.Ordinal) == true) {
                var json = await Resources.GetStringAsync("WhiteoutBotWrongApiKey.json");
                return new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new FakeHttpContent(json) };
            }

            throw new Exception("Unexpected uri was called during test. " + request.RequestUri?.PathAndQuery);
        }, CreateClient);

        await Should.ThrowAsync<HttpForbiddenException>(async () => await httpClient.GetGiftCodesAsync(TestCancellationToken));
    }
}
