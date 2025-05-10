using Borealis.Core.Contracts;
using Borealis.Core.Services;

namespace Borealis.Web.Extensions;

public static partial class IServiceCollectionExtensions {
    public static IServiceCollection AddCaptchaServices(this IServiceCollection services) {
        services.AddSingleton<ICaptchaSolverFactory, CaptchaSolverFactory>();
        services.AddKeyedTransient<CapSolverCaptchaSolver>(CaptchaSolverFactory.CapSolverKey);
        services.AddKeyedSingleton<KarlsCaptchaReaderCaptchaSolver>(CaptchaSolverFactory.KarlsCaptchaReaderKey);

        services.AddScoped(provider => {
            var factory = provider.GetRequiredService<ICaptchaSolverFactory>();
            return factory.CreateCaptchaSolver();
        });

        return services;
    }
}
