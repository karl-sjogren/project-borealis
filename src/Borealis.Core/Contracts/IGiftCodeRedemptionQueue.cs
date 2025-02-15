using Borealis.Core.Models;

namespace Borealis.Core.Contracts;

public interface IGiftCodeRedemptionQueue {
    Task AddToQueueAsync(Player player, GiftCode giftCode, CancellationToken cancellationToken);
    Task ProcessQueueAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<GiftCodeRedemptionQueueItem>> GetQueueAsync(CancellationToken cancellationToken);
    Task<int> GetQueueLengthAsync(CancellationToken cancellationToken);
}
