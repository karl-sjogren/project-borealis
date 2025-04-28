using Borealis.Core.Contracts;
using Borealis.Core.Models;
using Borealis.WhiteoutSurvivalHttpClient;
using Borealis.WhiteoutSurvivalHttpClient.Models;

namespace Borealis.Core.Services;

public class WhiteoutSurvivalService : IWhiteoutSurvivalService {
    private readonly IWhiteoutSurvivalHttpClient _whiteoutSurvivalHttpClient;
    private readonly ICapSolverHttpClient _capSolverHttpClient;
    private readonly ILogger<WhiteoutSurvivalService> _logger;

    public WhiteoutSurvivalService(
            IWhiteoutSurvivalHttpClient whiteoutSurvivalHttpClient,
            ICapSolverHttpClient capSolverHttpClient,
            ILogger<WhiteoutSurvivalService> logger) {
        _whiteoutSurvivalHttpClient = whiteoutSurvivalHttpClient;
        _capSolverHttpClient = capSolverHttpClient;
        _logger = logger;
    }

    public async Task<WhiteoutSurvivalPlayerResponse> GetPlayerInfoAsync(int playerId, CancellationToken cancellationToken) {
        var response = await _whiteoutSurvivalHttpClient.GetPlayerInfoAsync(playerId, cancellationToken);

        return response.Data;
    }

    public async Task<Result> RedeemGiftCodeAsync(int playerId, string giftCode, CancellationToken cancellationToken) {
        var playerResult = await _whiteoutSurvivalHttpClient.GetPlayerInfoAsync(playerId, cancellationToken); // We need to "sign in" the player
        var captchaResult = await _whiteoutSurvivalHttpClient.GetCaptchaAsync(playerId, cancellationToken);

        var captchaImage = GetCaptchaImageBytes(captchaResult.Data);
        if(captchaImage == null) {
            return Results.Failure("Failed to get captcha image bytes.");
        }

        var captchaResponse = await _capSolverHttpClient.ImageToTextAsync(captchaImage, cancellationToken);

        var captchaSolution = captchaResponse?.Solution?.Text;
        if(string.IsNullOrEmpty(captchaSolution)) {
            return Results.Failure("Failed to solve captcha.");
        }

        var redeemResult = await _whiteoutSurvivalHttpClient.RedeemGiftCodeAsync(playerId, giftCode, captchaSolution, cancellationToken);

        return redeemResult.ErrorCode switch {
            // Code success
            20000 or 40008 or 40011 => Results.Success("Gift code redeemed or was already redeemed."),
            // Claim limit reached, we can't redeem it but we can remember it
            40005 or 40007 => Results.Failure("Gift code expired."),
            40014 => Results.Failure("Gift code not found."),
            40004 => Results.Failure("Player not found."),
            40009 => Results.Failure("Player not logged in."),
            _ => Results.Failure($"Unknown error code: {redeemResult.ErrorCode}, message: {redeemResult.Message}"),
        };
    }

    private byte[]? GetCaptchaImageBytes(WhiteoutSurvivalCaptchaResponse data) {
        if(string.IsNullOrEmpty(data.ImageData) || !data.ImageData.StartsWith("data:image", StringComparison.OrdinalIgnoreCase) || !data.ImageData.Contains(",", StringComparison.Ordinal)) {
            _logger.LogError("Captcha image data is null or empty.");
            return null;
        }

        var base64Data = data.ImageData.Split(',')[1];

        var buffer = Convert.FromBase64String(base64Data);
        return buffer;
    }
}
