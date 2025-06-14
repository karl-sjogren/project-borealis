using Microsoft.Extensions.DependencyInjection;

namespace Borealis.WhiteoutSurvivalHttpClient.Tests;

public class ServiceCollectionExtensionsTests {
    [Fact]
    public void AddWhiteoutSurvivalHttpClient_AddsHttpClient() {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWhiteoutSurvivalHttpClient();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var httpClient = serviceProvider.GetService<IWhiteoutSurvivalHttpClient>();

        httpClient.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(1, 12)]
    [InlineData(2, 14)]
    [InlineData(3, 18)]
    [InlineData(4, 26)]
    [InlineData(5, 42)]
    [InlineData(6, 42)]
    [InlineData(7, 42)]
    public void CalculateRetryDelay_WhenCalled_ReturnsExpectedDelay(int attemptNumber, int expectedDelayInSeconds) {
        // Act
        var delay = ServiceCollectionExtensions.CalculateRetryDelay(attemptNumber);

        // Assert
        delay.ShouldBe(TimeSpan.FromSeconds(expectedDelayInSeconds));
    }
}
