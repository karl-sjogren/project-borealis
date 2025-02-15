using Borealis.Core.Models;

namespace Borealis.Web.ViewModels;

public class GiftCodeDetailsViewModel {
    public required GiftCode Code { get; init; }
    public required IReadOnlyCollection<GiftCodeRedemption> Redemptions { get; init; }
    public required IReadOnlyCollection<Player> PlayersNotRedeemedFor { get; init; }
}
