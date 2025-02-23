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
}
