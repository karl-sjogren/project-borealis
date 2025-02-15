using Borealis.Core.Models;

namespace Borealis.Web.ViewModels;

public class PlayersDetailsViewModel {
    public required Player Player { get; set; }
    public string? Notes { get; set; }
    public DateOnly? AwayUntil { get; set; }
    public IReadOnlyCollection<GiftCodeRedemption> RedeemedGiftCodes { get; set; } = [];
}
