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
        var captchaRetries = 0;
        const int maxCaptchaRetries = 3;

        WhiteoutSurvivalResponseWrapper? redeemResult = null;

        while(captchaRetries < maxCaptchaRetries) {
            var playerResult = await _whiteoutSurvivalHttpClient.GetPlayerInfoAsync(playerId, cancellationToken); // We need to "sign in" the player
            if(playerResult.ErrorCode != 0) {
                return Results.Failure($"Failed to get player info: {playerResult.ErrorCode}, message: {playerResult.Message}");
            }

            var captchaResult = await _whiteoutSurvivalHttpClient.GetCaptchaAsync(playerId, cancellationToken);

            if(captchaResult.ErrorCode == 40100) {
                _logger.LogWarning("Captcha requested too frequently, waiting longer... (attempt {Attempt})", captchaRetries);

                await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
                break;
            }

            var captchaImage = GetCaptchaImageBytes(captchaResult.Data);
            if(captchaImage == null) {
                return Results.Failure("Failed to get captcha image bytes.");
            }

            var captchaResponse = await _capSolverHttpClient.ImageToTextAsync(captchaImage, cancellationToken);

            var captchaSolution = captchaResponse?.Solution?.Text;
            if(string.IsNullOrEmpty(captchaSolution)) {
                return Results.Failure("Failed to solve captcha.");
            }

            redeemResult = await _whiteoutSurvivalHttpClient.RedeemGiftCodeAsync(playerId, giftCode, captchaSolution, cancellationToken);

            if(redeemResult.ErrorCode == 40103) {
                // Captcha failed, retry
                captchaRetries++;

                _logger.LogWarning("Captcha failed, retrying... (attempt {Attempt})", captchaRetries);

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }

            // Captcha didn't fail, break out of the loop
            break;
        }

        if(redeemResult == null) {
            return Results.Failure("Failed to even try redeeming gift code.");
        }

        return redeemResult.ErrorCode switch {
            // Code success
            0 or 20000 or 40008 or 40011 => Results.Success("Gift code redeemed or was already redeemed."),
            // Claim limit reached, we can't redeem it but we can remember it
            40005 or 40007 => Results.Failure("Gift code expired."),
            40014 => Results.Failure("Gift code not found."),
            40004 => Results.Failure("Player not found."),
            40009 => Results.Failure("Player not logged in."),
            40100 or 40103 => Results.Failure("Captcha failed."),
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
