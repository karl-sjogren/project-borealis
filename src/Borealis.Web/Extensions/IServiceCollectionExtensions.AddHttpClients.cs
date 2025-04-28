using Borealis.Core.Contracts;
using Borealis.Core.HttpClients;
using Borealis.Core.Options;
using Borealis.WhiteoutSurvivalHttpClient;
using Microsoft.Extensions.Options;

namespace Borealis.Web.Extensions;

public static partial class IServiceCollectionExtensions {
    public static IServiceCollection AddHttpClients(this IServiceCollection services) {
        services
            .AddHttpClient()
            .AddHttpClient<IWosLandHttpClient, WosLandHttpClient>((provider, client) => {
                var options = provider.GetRequiredService<IOptions<WosLandOptions>>().Value;

                client.DefaultRequestHeaders.Add("X-API-Key", options.ApiKey ?? string.Empty);
            });

        services.AddHttpClient<ICapSolverHttpClient, CapSolverHttpClient>((provider, client) => {
        });

        services.AddWhiteoutSurvivalHttpClient();

        return services;
    }
}
