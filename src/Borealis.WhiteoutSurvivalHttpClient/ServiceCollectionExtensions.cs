using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace Borealis.WhiteoutSurvivalHttpClient;

public static class ServiceCollectionExtensions {
    private static readonly MediaTypeWithQualityHeaderValue _jsonMediaType = new("application/json");

    public static IServiceCollection AddWhiteoutSurvivalHttpClient(this IServiceCollection services) {
        return services.AddWhiteoutSurvivalHttpClient(_ => { }, null, null);
    }

    public static IServiceCollection AddWhiteoutSurvivalHttpClient(this IServiceCollection services, Action<WhiteoutSurvivalOptions> configureOptions) {
        return services.AddWhiteoutSurvivalHttpClient(configureOptions, null, null);
    }

    internal static IServiceCollection AddWhiteoutSurvivalHttpClient(
            this IServiceCollection services,
            Action<WhiteoutSurvivalOptions> configureOptions,
            int? retryCount,
            Func<int, TimeSpan>? sleepDurationProvider) {
        services.Configure(configureOptions);

        services.TryAddSingleton(TimeProvider.System);

        IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(static msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(retryCount ?? 10, sleepDurationProvider ?? (static retryAttempt => TimeSpan.FromSeconds(10 + Math.Pow(2, retryAttempt))));
        }

        services
            .AddHttpClient<IWhiteoutSurvivalHttpClient, WhiteoutSurvivalHttpClient>()
            .ConfigureHttpClient(static (serviceProvider, client) => {
                var options = serviceProvider.GetRequiredService<IOptions<WhiteoutSurvivalOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(_jsonMediaType);
                client.DefaultRequestHeaders.Add("Origin", options.OriginUrl);
            })
            .AddPolicyHandler(GetRetryPolicy());

        return services;
    }
}
