using Borealis.Core.Models;
using Borealis.Core.Requests;

namespace Borealis.Core.Contracts;

public interface IGiftCodeService {
    Task<Result<GiftCode>> GetByIdAsync(Guid giftCodeId, CancellationToken cancellationToken);
    Task<PagedResult<GiftCode>> GetPagedAsync(GiftCodeQuery query, CancellationToken cancellationToken);
    Task<Result> AddGiftCodeAsync(string giftCode, CancellationToken cancellationToken);
    Task<Result> RedeemGiftCodeAsync(int whiteoutSurvivalPlayerId, string giftCode, CancellationToken cancellationToken);
    Task<Result> EnqueueGiftCodeAsync(Guid giftCodeId, CancellationToken cancellationToken);
    Task<ICollection<GiftCodeRedemption>> GetRedemptionsForPlayerAsync(Guid giftCodeId, CancellationToken cancellationToken);
    Task<Result<GiftCode>> UpdateAsync(GiftCode giftCode, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid giftCodeId, CancellationToken cancellationToken);
}
