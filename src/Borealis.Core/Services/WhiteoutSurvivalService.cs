using Borealis.Core.Contracts;
using Borealis.Core.Models;
using Borealis.WhiteoutSurvivalHttpClient;
using Borealis.WhiteoutSurvivalHttpClient.Models;

namespace Borealis.Core.Services;

public class WhiteoutSurvivalService : IWhiteoutSurvivalService {
    private readonly IWhiteoutSurvivalHttpClient _whiteoutSurvivalHttpClient;
    private readonly ICaptchaSolver _captchaSolver;
    private readonly ILogger<WhiteoutSurvivalService> _logger;

    public WhiteoutSurvivalService(
            IWhiteoutSurvivalHttpClient whiteoutSurvivalHttpClient,
            ICaptchaSolver captchaSolver,
            ILogger<WhiteoutSurvivalService> logger) {
        _whiteoutSurvivalHttpClient = whiteoutSurvivalHttpClient;
        _captchaSolver = captchaSolver;
        _logger = logger;
    }

    public async Task<WhiteoutSurvivalPlayerResponse> GetPlayerInfoAsync(int playerId, CancellationToken cancellationToken) {
        var response = await _whiteoutSurvivalHttpClient.GetPlayerInfoAsync(playerId, cancellationToken);

        return response.Data;
    }

    public async Task<Result> RedeemGiftCodeAsync(int playerId, string giftCode, CancellationToken cancellationToken) {
        var captchaRetries = 0;
        const int maxCaptchaRetries = 10;

        WhiteoutSurvivalResponseWrapper? redeemResult = null;

        while(captchaRetries < maxCaptchaRetries) {
            var playerResult = await _whiteoutSurvivalHttpClient.GetPlayerInfoAsync(playerId, cancellationToken); // We need to "sign in" the player
            if(playerResult.ErrorCode != 0) {
                return Results.Failure(new GiftCodePlayerErrorMessage(playerId, playerResult.ErrorCode, playerResult.Message));
            }

            var captchaResult = await _whiteoutSurvivalHttpClient.GetCaptchaAsync(playerId, cancellationToken);

            if(captchaResult.ErrorCode == 40100) {
                _logger.LogWarning("Captcha requested too frequently, waiting longer... (attempt {Attempt})", captchaRetries);

                await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
                continue;
            }

            var captchaImage = GetCaptchaImageBytes(captchaResult.Data);
            if(captchaImage == null) {
                return Results.Failure(new FailedToGetCaptchaMessage());
            }

            var captchaSolution = await _captchaSolver.SolveCaptchaAsync(captchaImage, cancellationToken);
            if(string.IsNullOrEmpty(captchaSolution)) {
                return Results.Failure(new FailedToSolveCaptchaMessage());
            }

            redeemResult = await _whiteoutSurvivalHttpClient.RedeemGiftCodeAsync(playerId, giftCode, captchaSolution, cancellationToken);

            if(redeemResult.ErrorCode == 40103) {
                // Captcha failed, retry
                captchaRetries++;

                _logger.LogWarning("Captcha failed, retrying... (attempt {Attempt})", captchaRetries);

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                continue;
            }

            // Captcha didn't fail, break out of the loop
            break;
        }

        if(redeemResult == null) {
            return Results.Failure(new WhiteoutSurvivalMessage("UnknownGiftCodeError", "Redeem result is null after captcha retries."));
        }

        if(captchaRetries >= maxCaptchaRetries) {
            return Results.Failure(new WhiteoutSurvivalMessage("RedeemGiftCodeFailed", "Failed to redeem gift code after {0} attempts.", maxCaptchaRetries));
        }

        // This is a special case that seems to happen sometimes
        // while the error code is 0, the message is "Sign Error"
        if(redeemResult.Message == "Sign Error") {
            return Results.Failure(new GiftCodeSignErrorMessage());
        }

        return redeemResult.ErrorCode switch {
            // Code success
            0 or 20000 or 40008 or 40011 => Results.Success(new GiftCodeRedeemedMessage(giftCode)),
            40005 => Results.Failure(new GiftCodeRedeemLimitReachedMessage(giftCode)),
            40007 => Results.Failure(new GiftCodeExpiredMessage(giftCode)),
            40014 => Results.Failure(new GiftCodeNotFoundMessage(giftCode)),
            40004 => Results.Failure(new GiftCodePlayerNotFoundMessage(playerId)),
            40009 => Results.Failure(new GiftCodePlayerNotLoggedInMessage(playerId)),
            40100 or 40103 => Results.Failure(new GiftCodeCaptchaFailedMessage(giftCode)),
            _ => Results.Failure(new WhiteoutSurvivalMessage("UnknownGiftCodeError", "Unknown error code: {0}, message: {1}", redeemResult.ErrorCode, redeemResult.Message)),
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
