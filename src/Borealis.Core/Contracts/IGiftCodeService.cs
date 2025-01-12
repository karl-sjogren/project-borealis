using Borealis.Core.Models;
using Borealis.Core.Requests;

namespace Borealis.Core.Contracts;

public interface IGiftCodeService {
    Task<Result<GiftCode>> GetByIdAsync(Guid playerId, CancellationToken cancellationToken);
    Task<PagedResult<GiftCode>> GetPagedAsync(GiftCodeQuery query, CancellationToken cancellationToken);
    Task<Result> AddGiftCodeAsync(string giftCode, CancellationToken cancellationToken);
    Task<Result> RedeemGiftCodeAsync(int whiteoutSurvivalPlayerId, string giftCode, CancellationToken cancellationToken);
}
