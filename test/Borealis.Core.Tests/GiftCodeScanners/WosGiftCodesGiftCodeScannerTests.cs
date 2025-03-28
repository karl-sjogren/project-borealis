using System.Net;
using Borealis.Core.GiftCodeScanners;
using Borealis.TestHelpers;
using Borealis.TestHelpers.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Borealis.Core.Tests.GiftCodeScanners;

public class WosGiftCodesGiftCodeScannerTests {
    [Fact]
    public async Task ScanGiftCodes_WhenClientReturnsCodes_ShouldJustReturnCodesAsync() {
        // Arrange
        var httpMessageHandler = new FakeHttpMessageHandler(async (request, cancellationToken) => {
            if(request.RequestUri?.PathAndQuery.EndsWith("/rss.php", StringComparison.Ordinal) == true) {
                var json = await Resources.GetStringAsync("WosGiftCodesSuccess.xml");
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new FakeHttpContent(json) };
            }

            throw new Exception("Unexpected uri was called during test. " + request.RequestUri?.PathAndQuery);
        });

        var httpClient = new HttpClient(httpMessageHandler);

        var scanner = new WosGiftCodesGiftCodeScanner(httpClient, NullLogger<WosGiftCodesGiftCodeScanner>.Instance);

        // Act
        var result = await scanner.ScanGiftCodesAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldContain("code-1");
        result.ShouldContain("code-2");
    }

    [Fact]
    public async Task ScanGiftCodes_WhenClientThrowsHttpRequestException_ShouldReturnEmptyListAsync() {
        // Arrange
        var httpMessageHandler = new FakeHttpMessageHandler((request, cancellationToken) => {
            if(request.RequestUri?.PathAndQuery.EndsWith("/rss.php", StringComparison.Ordinal) == true) {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            throw new Exception("Unexpected uri was called during test. " + request.RequestUri?.PathAndQuery);
        });

        var httpClient = new HttpClient(httpMessageHandler);

        var fakeLogger = new FakeLogger<WosGiftCodesGiftCodeScanner>();
        var scanner = new WosGiftCodesGiftCodeScanner(httpClient, fakeLogger);

        // Act
        var result = await scanner.ScanGiftCodesAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        var logs = fakeLogger.Collector.GetSnapshot();
        logs.ShouldNotBeEmpty();
        logs.ShouldContain(log => log.Level == LogLevel.Error && log.Message.Contains("Failed to retrieve gift code rss source."));
    }

    [Theory]
    [InlineData("WosGiftCodesEmpty.xml", "Gift code rss source is empty.")]
    [InlineData("WosGiftCodesInvalid.xml", "Failed to parse gift code rss source.")]
    public async Task ScanGiftCodes_WhenClientReturnsEmptyOrInvalidJson_ShouldReturnEmptyListAsync(string resourceName, string expectedLogMessage) {
        // Arrange
        var httpMessageHandler = new FakeHttpMessageHandler(async (request, cancellationToken) => {
            if(request.RequestUri?.PathAndQuery.EndsWith("/rss.php", StringComparison.Ordinal) == true) {
                var json = await Resources.GetStringAsync(resourceName);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new FakeHttpContent(json) };
            }

            throw new Exception("Unexpected uri was called during test. " + request.RequestUri?.PathAndQuery);
        });

        var httpClient = new HttpClient(httpMessageHandler);

        var fakeLogger = new FakeLogger<WosGiftCodesGiftCodeScanner>();
        var scanner = new WosGiftCodesGiftCodeScanner(httpClient, fakeLogger);

        // Act
        var result = await scanner.ScanGiftCodesAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        var logs = fakeLogger.Collector.GetSnapshot();
        logs.ShouldNotBeEmpty();
        logs.ShouldContain(log => log.Level == LogLevel.Error && log.Message == expectedLogMessage);
    }
}
