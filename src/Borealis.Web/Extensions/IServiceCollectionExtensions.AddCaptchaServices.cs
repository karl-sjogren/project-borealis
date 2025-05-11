using Borealis.Core.Contracts;
using Borealis.Core.Options;
using Borealis.Core.Services;
using Karls.CaptchaReader;

namespace Borealis.Web.Extensions;

public static partial class IServiceCollectionExtensions {
    public static IServiceCollection AddCaptchaServices(this IServiceCollection services) {
        services.AddSingleton<ICaptchaSolverFactory, CaptchaSolverFactory>();
        services.AddKeyedTransient<ICaptchaSolver, CapSolverCaptchaSolver>(BorealisOptions.CapSolverKey);
        services.AddKeyedSingleton<ICaptchaSolver, KarlsCaptchaReaderCaptchaSolver>(BorealisOptions.KarlsCaptchaReaderKey);

        services.AddSingleton<IOcrReader, DdddOcrReader>();

        services.AddScoped(provider => {
            var factory = provider.GetRequiredService<ICaptchaSolverFactory>();
            return factory.CreateCaptchaSolver();
        });

        return services;
    }
}
