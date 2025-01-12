namespace Borealis.Core.Models;

public class GiftCode : EntityBase {
    public required string Code { get; set; }
    public bool IsExpired { get; set; }
    public ICollection<GiftCodeRedemption> Redemptions { get; set; } = [];
}
