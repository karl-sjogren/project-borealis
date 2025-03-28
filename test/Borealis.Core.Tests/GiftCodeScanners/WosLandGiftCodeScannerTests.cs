using Borealis.Core.Contracts;
using Borealis.Core.GiftCodeScanners;
using Borealis.WhiteoutSurvivalHttpClient.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Borealis.Core.Tests.GiftCodeScanners;

public class WosLandGiftCodeScannerTests {
    [Fact]
    public async Task ScanGiftCodes_WhenClientReturnsExpectedFeedData_ShouldJustReturnCodesAsync() {
        // Arrange
        var httpClient = A.Fake<IWosLandHttpClient>();

        A.CallTo(() => httpClient.GetGiftCodesAsync(A<CancellationToken>._))
            .Returns(new List<string> { "code1", "code2" });

        var scanner = new WosLandGiftCodeScanner(httpClient, NullLogger<WosLandGiftCodeScanner>.Instance);

        // Act
        var result = await scanner.ScanGiftCodesAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldContain("code1");
        result.ShouldContain("code2");
    }

    [Fact]
    public async Task ScanGiftCodes_WhenClientThrowsUnauthorizedException_ShouldReturnEmptyListAsync() {
        // Arrange
        var httpClient = A.Fake<IWosLandHttpClient>();

        A.CallTo(() => httpClient.GetGiftCodesAsync(A<CancellationToken>._))
            .Throws(new HttpUnauthorizedException());

        var fakeLogger = new FakeLogger<WosLandGiftCodeScanner>();
        var scanner = new WosLandGiftCodeScanner(httpClient, fakeLogger);

        // Act
        var result = await scanner.ScanGiftCodesAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        var logs = fakeLogger.Collector.GetSnapshot();
        logs.ShouldNotBeEmpty();
        logs.ShouldContain(log => log.Level == LogLevel.Error && log.Message.Contains("Failed to scan gift codes from WOS Land. Most likely due to an invalid API Key."));
    }
}
