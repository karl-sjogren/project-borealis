using Microsoft.Extensions.Options;

namespace Borealis.Core.Options;

public class BorealisOptionsValidator : IValidateOptions<BorealisOptions> {
    public ValidateOptionsResult Validate(string? name, BorealisOptions options) {
        var applicationUrl = options.ApplicationUrl;
        if(string.IsNullOrEmpty(applicationUrl)) {
            return ValidateOptionsResult.Fail("ApplicationUrl is not set.");
        }

        if(!Uri.TryCreate(applicationUrl, UriKind.Absolute, out _)) {
            return ValidateOptionsResult.Fail("ApplicationUrl is not a valid absolute URI.");
        }

        if(!applicationUrl.EndsWith("/", StringComparison.Ordinal)) {
            return ValidateOptionsResult.Fail("ApplicationUrl must end with a trailing slash.");
        }

        var captchaSolver = options.CaptchaSolver;
        if(string.IsNullOrEmpty(captchaSolver)) {
            return ValidateOptionsResult.Fail("CaptchaSolver is not set.");
        }

        var validCaptchaSolvers = new[] {
            BorealisOptions.CapSolverKey,
            BorealisOptions.KarlsCaptchaReaderKey
        };

        if(!validCaptchaSolvers.Contains(captchaSolver)) {
            return ValidateOptionsResult.Fail($"Invalid CaptchaSolver: {captchaSolver}. Valid options are: {string.Join(", ", validCaptchaSolvers)}");
        }

        return ValidateOptionsResult.Success;
    }
}
