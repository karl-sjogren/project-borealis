using Borealis.WhiteoutSurvivalHttpClient.Models;

namespace Borealis.WhiteoutSurvivalHttpClient;

public interface IWhiteoutSurvivalHttpClient {
    Task<WhiteoutSurvivalResponseWrapper<WhiteoutSurvivalPlayerResponse>> GetPlayerInfoAsync(int playerId, CancellationToken cancellationToken);
    Task<WhiteoutSurvivalResponseWrapper<WhiteoutSurvivalCaptchaResponse>> GetCaptchaAsync(int playerId, CancellationToken cancellationToken);
    Task<WhiteoutSurvivalResponseWrapper> RedeemGiftCodeAsync(int playerId, string giftCode, string captchaCode, CancellationToken cancellationToken);
}
