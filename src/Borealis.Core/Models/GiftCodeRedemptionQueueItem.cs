namespace Borealis.Core.Models;

public record GiftCodeRedemptionQueueItem {
    public required Player Player { get; init; }
    public required GiftCode GiftCode { get; init; }
    public required DateTimeOffset EnqueuedAt { get; init; }
}
