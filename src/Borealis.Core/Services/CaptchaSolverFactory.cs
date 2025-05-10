using Borealis.Core.Contracts;
using Borealis.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Borealis.Core.Services;

public class CaptchaSolverFactory : ICaptchaSolverFactory {
    private readonly BorealisOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public const string CapSolverKey = "CapSolver";
    public const string KarlsCaptchaReaderKey = "KarlsCaptchaReader";

    public CaptchaSolverFactory(IOptions<BorealisOptions> optionsAccessor, IServiceProvider serviceProvider) {
        _options = optionsAccessor.Value;
        _serviceProvider = serviceProvider;
    }

    public ICaptchaSolver CreateCaptchaSolver() {
        if(_options.CaptchaSolver.Equals(CapSolverKey, StringComparison.OrdinalIgnoreCase)) {
            return _serviceProvider.GetRequiredKeyedService<ICaptchaSolver>(CapSolverKey);
        } else if(_options.CaptchaSolver.Equals(KarlsCaptchaReaderKey, StringComparison.OrdinalIgnoreCase)) {
            return _serviceProvider.GetRequiredKeyedService<ICaptchaSolver>(KarlsCaptchaReaderKey);
        } else {
            throw new NotSupportedException($"Captcha solver '{_options.CaptchaSolver}' is not supported.");
        }
    }
}
