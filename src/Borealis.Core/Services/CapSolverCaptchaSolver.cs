using Borealis.Core.Contracts;

namespace Borealis.Core.Services;

public class CapSolverCaptchaSolver : ICaptchaSolver {
    private readonly ICapSolverHttpClient _capSolverHttpClient;
    private readonly ILogger<CapSolverCaptchaSolver> _logger;

    public CapSolverCaptchaSolver(
            ICapSolverHttpClient capSolverHttpClient,
            ILogger<CapSolverCaptchaSolver> logger) {
        _capSolverHttpClient = capSolverHttpClient;
        _logger = logger;
    }

    public async Task<string?> SolveCaptchaAsync(byte[] captchaImage, CancellationToken cancellationToken) {
        var captchaResponse = await _capSolverHttpClient.ImageToTextAsync(captchaImage, cancellationToken);
        if(captchaResponse is null) {
            _logger.LogError("Failed to get captcha response from CapSolver.");
            return null;
        }

        if(captchaResponse.Status != "ready") {
            _logger.LogError("Captcha response is not ready. Status: {Status}", captchaResponse.Status);
            return null;
        }

        return captchaResponse?.Solution?.Text;
    }
}
