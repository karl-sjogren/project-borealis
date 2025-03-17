using System.Net.Http.Headers;
using Borealis.Core.Contracts;
using Borealis.Core.HttpClients;
using Borealis.Core.Options;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace Borealis.Web.Extensions;

public static partial class IServiceCollectionExtensions {
    public static IServiceCollection AddHttpClients(this IServiceCollection services) {
        services.AddWhiteoutSurvivalHttpClient();

        return services;
    }
}
