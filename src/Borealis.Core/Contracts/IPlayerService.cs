using Borealis.Core.Models;
using Borealis.Core.Requests;

namespace Borealis.Core.Contracts;

public interface IPlayerService {
    Task<Result<WhiteoutSurvivalPlayer>> GetByIdAsync(Guid playerId, CancellationToken cancellationToken);
    Task<Result<WhiteoutSurvivalPlayer>> GetByExternalIdAsync(int whiteoutSurvivalPlayerId, CancellationToken cancellationToken);
    Task<PagedResult<WhiteoutSurvivalPlayer>> GetPagedAsync(PlayerQuery query, CancellationToken cancellationToken);
    Task<Result<WhiteoutSurvivalPlayer>> SynchronizePlayerAsync(int whiteoutSurvivalPlayerId, CancellationToken cancellationToken);
    Task<Result<WhiteoutSurvivalPlayer>> SetPlayerNotesAsync(Guid playerId, string notes, CancellationToken cancellationToken);
}
