using Borealis.Core.Models;

namespace Borealis.Core.Contracts;

public interface IPlayerService {
    Task<Result<WhiteoutSurvivalPlayer>> GetByIdAsync(Guid playerId, CancellationToken cancellationToken);
    Task<PagedResult<WhiteoutSurvivalPlayer>> GetPagedAsync(int pageIndex, int pageSize, CancellationToken cancellationToken);
    Task<Result<WhiteoutSurvivalPlayer>> SynchronizePlayerAsync(int whiteoutSurvivalPlayerId, CancellationToken cancellationToken);
    Task<Result<WhiteoutSurvivalPlayer>> SetPlayerNotesAsync(Guid playerId, string notes, CancellationToken cancellationToken);
}
