namespace Borealis.Core.Contracts;

public interface ICaptchaSolver {
    Task<string?> SolveCaptchaAsync(byte[] captchaImage, CancellationToken cancellationToken);
}
