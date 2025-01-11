using Borealis.Core.HttpClients.Models;

namespace Borealis.Core.Contracts;

public interface IWhiteoutSurvivalHttpClient {
    Task<WhiteoutSurvivalResponseWrapper<WhiteoutSurvivalPlayerResponse>> GetPlayerInfoAsync(int playerId, CancellationToken cancellationToken);
    Task<WhiteoutSurvivalResponseWrapper> RedeemGiftCodeAsync(int playerId, string giftCode, CancellationToken cancellationToken);
}
