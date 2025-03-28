using System.Net;
using Borealis.TestHelpers;
using Borealis.TestHelpers.Http;
using Microsoft.Extensions.Options;

namespace Borealis.WhiteoutSurvivalHttpClient.Tests;

public class WhiteoutSurvivalHttpClientTests {
    private static WhiteoutSurvivalHttpClient CreateClient(HttpClient httpClient) => new(httpClient, Options.Create<WhiteoutSurvivalOptions>(new()), TimeProvider.System, NullLogger<WhiteoutSurvivalHttpClient>.Instance);

    [Fact]
    public async Task GetPlayerInfoSuccessAsync() {
        var httpClient = HttpClientActivator<WhiteoutSurvivalHttpClient>.GetClient(async (request, _) => {
            if(request.RequestUri?.PathAndQuery.EndsWith("/player", StringComparison.Ordinal) == true) {
                var json = await Resources.GetStringAsync("PlayerSuccess.json");
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new FakeHttpContent(json) };
            }

            throw new Exception("Unexpected uri was called during test. " + request.RequestUri?.PathAndQuery);
        }, CreateClient);

        var result = await httpClient.GetPlayerInfoAsync(123, TestCancellationToken);

        result.ShouldNotBeNull();
        result.Code.ShouldBe(0);
        result.ErrorCode.ShouldBe(0);

        var player = result.Data;
        player.ShouldNotBeNull();
        player.FurnaceId.ShouldBe(123744924);
        player.Name.ShouldBe("Prototype");
        player.FurnaceLevel.ShouldBe(60);
        player.State.ShouldBe(851);
    }

    [Fact]
    public async Task GetPlayerInfoNotFoundAsync() {
        var httpClient = HttpClientActivator<WhiteoutSurvivalHttpClient>.GetClient(async (request, _) => {
            if(request.RequestUri?.PathAndQuery.EndsWith("/player", StringComparison.Ordinal) == true) {
                var json = await Resources.GetStringAsync("PlayerNotFound.json");
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new FakeHttpContent(json) };
            }

            throw new Exception("Unexpected uri was called during test. " + request.RequestUri?.PathAndQuery);
        }, CreateClient);

        var result = await httpClient.GetPlayerInfoAsync(123, TestCancellationToken);

        result.ShouldNotBeNull();
        result.Code.ShouldBe(1);
        result.ErrorCode.ShouldBe(40004);

        var player = result.Data;
        player.ShouldBeNull();
    }

    [Theory]
    [InlineData("CodeSuccess.json", 0, 20000)]
    [InlineData("CodeAlreadyRedeemed.json", 1, 40011)]
    [InlineData("CodeClaimLimitReached.json", 1, 40005)]
    [InlineData("CodeExpired.json", 1, 40007)]
    [InlineData("CodeNotFound.json", 1, 40014)]
    [InlineData("CodeNotLoggedIn.json", 1, 40009)]
    [InlineData("CodeReceived.json", 1, 40008)]
    public async Task RedeemGiftCodeVariantsAsync(string resourceName, int expectedCode, int expectedErrorCode) {
        var httpClient = HttpClientActivator<WhiteoutSurvivalHttpClient>.GetClient(async (request, _) => {
            if(request.RequestUri?.PathAndQuery.EndsWith("/gift_code", StringComparison.Ordinal) == true) {
                var json = await Resources.GetStringAsync(resourceName);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new FakeHttpContent(json) };
            }

            throw new Exception("Unexpected uri was called during test. " + request.RequestUri?.PathAndQuery);
        }, CreateClient);

        var result = await httpClient.RedeemGiftCodeAsync(123, "sassy-zebra", TestCancellationToken);

        result.ShouldNotBeNull();
        result.Code.ShouldBe(expectedCode);
        result.ErrorCode.ShouldBe(expectedErrorCode);
    }
}
