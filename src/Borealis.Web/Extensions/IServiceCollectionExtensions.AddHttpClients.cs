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
        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        services
            .AddHttpClient<IWhiteoutSurvivalHttpClient, WhiteoutSurvivalHttpClient>()
            .ConfigureHttpClient((serviceProvider, client) => {
                var options = serviceProvider.GetRequiredService<IOptions<WhiteoutSurvivalOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Origin", options.OriginUrl);
            })
            .AddPolicyHandler(GetRetryPolicy());

        return services;
    }
}
