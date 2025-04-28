using Borealis.Core.Models;
using Borealis.WhiteoutSurvivalHttpClient.Models;

namespace Borealis.Core.Contracts;

public interface IWhiteoutSurvivalService {
    Task<WhiteoutSurvivalPlayerResponse> GetPlayerInfoAsync(int playerId, CancellationToken cancellationToken);
    Task<Result> RedeemGiftCodeAsync(int playerId, string giftCode, CancellationToken cancellationToken);
}
