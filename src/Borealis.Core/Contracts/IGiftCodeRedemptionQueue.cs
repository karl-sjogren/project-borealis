using Borealis.Core.Models;

namespace Borealis.Core.Contracts;

public interface IGiftCodeRedemptionQueue {
    Task AddToQueueAsync(WhiteoutSurvivalPlayer player, GiftCode giftCode, CancellationToken cancellationToken);
    Task ProcessQueueAsync(CancellationToken cancellationToken);
}
