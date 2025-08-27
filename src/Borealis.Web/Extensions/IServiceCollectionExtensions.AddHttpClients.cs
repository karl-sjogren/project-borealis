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
            .AddHttpClient<IWhiteoutBotHttpClient, WhiteoutBotHttpClient>((provider, client) => {
                var options = provider.GetRequiredService<IOptions<WhiteoutBotOptions>>().Value;

                client.BaseAddress = new Uri("https+http://whiteout-bot");
                client.DefaultRequestHeaders.Add("X-API-Key", options.ApiKey ?? string.Empty);
            });

        services
            .AddHttpClient<IWhiteoutSurvivalHttpClientProxy, WhiteoutSurvivalHttpClientProxy>((provider, client) => {
                var options = provider.GetRequiredService<IOptions<WhiteoutSurvivalProxyOptions>>().Value;

                client.BaseAddress = new Uri("https+http://borealis-wos-proxy/");
                client.DefaultRequestHeaders.Add("x-functions-key", options.FunctionKey ?? string.Empty);
            });

        services.AddHttpClient<ICapSolverHttpClient, CapSolverHttpClient>((_, client) => {
            client.BaseAddress = new Uri("https://api.capsolver.com/");
        });

        services.AddWhiteoutSurvivalHttpClient();

        return services;
    }
}
