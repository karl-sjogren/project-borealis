namespace Borealis.Core.Models;

public class GiftCodeRedemption {
    public Guid Id { get; set; }
    public required Guid GiftCodeId { get; set; }
    public required GiftCode GiftCode { get; set; }
    public required Guid PlayerId { get; set; }
    public required WhiteoutSurvivalPlayer Player { get; set; }
    public required DateTimeOffset RedeemedAt { get; set; }
}
