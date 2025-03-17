using Borealis.WhiteoutSurvivalHttpClient;

namespace Borealis.Web.Extensions;

public static partial class IServiceCollectionExtensions {
    public static IServiceCollection AddHttpClients(this IServiceCollection services) {
        services.AddHttpClient();
        services.AddWhiteoutSurvivalHttpClient();

        return services;
    }
}
