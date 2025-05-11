using Borealis.Core.Contracts;
using Borealis.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Borealis.Core.Services;

public class CaptchaSolverFactory : ICaptchaSolverFactory {
    private readonly BorealisOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public CaptchaSolverFactory(IOptions<BorealisOptions> optionsAccessor, IServiceProvider serviceProvider) {
        _options = optionsAccessor.Value;
        _serviceProvider = serviceProvider;
    }

    public ICaptchaSolver CreateCaptchaSolver() {
        return _serviceProvider.GetRequiredKeyedService<ICaptchaSolver>(_options.CaptchaSolver);
    }
}
