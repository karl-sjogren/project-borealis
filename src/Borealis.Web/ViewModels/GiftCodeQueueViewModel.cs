using Borealis.Core.Models;

namespace Borealis.Web.ViewModels;

public class GiftCodeQueueViewModel {
    public required IReadOnlyCollection<GiftCodeRedemptionQueueItem> Items { get; init; }
}
