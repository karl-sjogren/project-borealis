using Borealis.WhiteoutSurvivalHttpClient.Models;

namespace Borealis.Core.Contracts;

public interface IWhiteoutSurvivalHttpClientProxy {
    Task<WhiteoutSurvivalResponseWrapper<WhiteoutSurvivalPlayerResponse>> GetPlayerInfoAsync(int playerId, CancellationToken cancellationToken);
}
