using Borealis.Core.Models;
using Borealis.Core.Requests;

namespace Borealis.Core.Contracts;

public interface IPlayerService {
    Task<Result<Player>> GetByIdAsync(Guid playerId, CancellationToken cancellationToken);
    Task<Result<Player>> GetByExternalIdAsync(int whiteoutSurvivalPlayerId, CancellationToken cancellationToken);
    Task<PagedResult<Player>> GetPagedAsync(PlayerQuery query, CancellationToken cancellationToken);
    Task<Result<Player>> SynchronizePlayerAsync(int whiteoutSurvivalPlayerId, bool addAsInAlliance, CancellationToken cancellationToken);
    Task<Result<Player>> UpdateAsync(Player player, CancellationToken cancellationToken);
}
