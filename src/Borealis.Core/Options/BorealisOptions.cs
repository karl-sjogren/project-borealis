namespace Borealis.Core.Options;

public class BorealisOptions {
    public required string ApplicationUrl { get; set; }
    public required bool UseHttpRedirection { get; set; }
    public required string CaptchaSolver { get; set; } = KarlsCaptchaReaderKey;

    public const string CapSolverKey = "CapSolver";
    public const string KarlsCaptchaReaderKey = "KarlsCaptchaReader";
}
