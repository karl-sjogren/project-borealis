using Borealis.Core.Contracts;
using Karls.CaptchaReader;

namespace Borealis.Core.Services;

public class KarlsCaptchaReaderCaptchaSolver : ICaptchaSolver {
    private readonly IOcrReader _ocrReader;
    private readonly ILogger<KarlsCaptchaReaderCaptchaSolver> _logger;

    public KarlsCaptchaReaderCaptchaSolver(IOcrReader ocrReader, ILogger<KarlsCaptchaReaderCaptchaSolver> logger) {
        _ocrReader = ocrReader;
        _logger = logger;
    }

    public async Task<string?> SolveCaptchaAsync(byte[] captchaImage, CancellationToken cancellationToken) {
        return await _ocrReader.ReadTextAsync(captchaImage, cancellationToken);
    }
}
