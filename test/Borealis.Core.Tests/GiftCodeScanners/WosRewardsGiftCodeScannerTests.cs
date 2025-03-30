using System.Net;
using Borealis.Core.GiftCodeScanners;
using Borealis.TestHelpers;
using Borealis.TestHelpers.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Borealis.Core.Tests.GiftCodeScanners;

public class WosRewardsGiftCodeScannerTests {
    [Theory]
    [InlineData("WosRewardsSourceFull.html")]
    [InlineData("WosRewardsSourceMinimal.html")]
    public async Task ScanGiftCodes_WhenClientExpectedHtml_ShouldJustReturnCodesAsync(string resourceName) {
        // Arrange
        var httpMessageHandler = new FakeHttpMessageHandler(async (request, cancellationToken) => {
            if(request.RequestUri?.PathAndQuery == "/") {
                var json = await Resources.GetStringAsync(resourceName);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new FakeHttpContent(json) };
            }

            throw new Exception("Unexpected uri was called during test. " + request.RequestUri?.PathAndQuery);
        });

        var httpClient = new HttpClient(httpMessageHandler);

        var scanner = new WosRewardsGiftCodeScanner(httpClient, NullLogger<WosRewardsGiftCodeScanner>.Instance);

        // Act
        var result = await scanner.ScanGiftCodesAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldContain("WOSHappyBirthday");
        result.ShouldContain("2ndAnniversary");
        result.ShouldContain("woshjm25");
        result.ShouldContain("woshazzisss");
        result.ShouldContain("woskjk76");
    }

    [Theory]
    [InlineData("WosRewardsSourceEmptyResponse.html", "Failed to retrieve gift code page source.", LogLevel.Error)]
    [InlineData("WosRewardsSourceMissingHeader.html", "Gift code header not found.", LogLevel.Error)]
    [InlineData("WosRewardsSourceMissingContainer.html", "Gift code container not found.", LogLevel.Error)]
    [InlineData("WosRewardsSourceNoCodes.html", "No gift codes found.", LogLevel.Warning)]
    public async Task ScanGiftCodes_WhenClientReturnsEmptyOrInvalidJson_ShouldReturnEmptyListAsync(string resourceName, string expectedLogMessage, LogLevel expectedLogLevel) {
        // Arrange
        var httpMessageHandler = new FakeHttpMessageHandler(async (request, cancellationToken) => {
            if(request.RequestUri?.PathAndQuery == "/") {
                var json = await Resources.GetStringAsync(resourceName);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new FakeHttpContent(json) };
            }

            throw new Exception("Unexpected uri was called during test. " + request.RequestUri?.PathAndQuery);
        });

        var httpClient = new HttpClient(httpMessageHandler);

        var fakeLogger = new FakeLogger<WosRewardsGiftCodeScanner>();
        var scanner = new WosRewardsGiftCodeScanner(httpClient, fakeLogger);

        // Act
        var result = await scanner.ScanGiftCodesAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        var logs = fakeLogger.Collector.GetSnapshot();
        logs.ShouldNotBeEmpty();
        logs.ShouldContain(log => log.Level == expectedLogLevel && log.Message == expectedLogMessage);
    }
}
