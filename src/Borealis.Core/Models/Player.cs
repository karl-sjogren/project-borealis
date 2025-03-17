namespace Borealis.Core.Models;

public class Player : EntityBase {
    public required int ExternalId { get; set; }
    public required string Name { get; set; }
    public required int State { get; set; }
    public required int FurnaceLevel { get; set; }
    public required bool IsInAlliance { get; set; }
    public bool ForceRedeemGiftCodes { get; set; }
    public DateOnly? AwayUntil { get; set; }
    public string? Notes { get; set; }
    public IList<PlayerNameHistoryEntry> PreviousNames { get; set; } = [];
    public IList<PlayerStateHistoryEntry> PreviousStates { get; set; } = [];

    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

    public string HtmlNotes => Notes?.Replace("\n", "<br />", StringComparison.OrdinalIgnoreCase) ?? "";

    public string FurnaceLevelString {
        get {
            if(FurnaceLevel <= 34) {
                var clampedLevel = Math.Clamp(FurnaceLevel, 0, 30);
                return clampedLevel.ToString();
            }

            var rank = (FurnaceLevel - 30) / 5;
            return $"FC{rank}";
        }
    }

    public bool HasFireCrystalFurnace => FurnaceLevel >= 35;

    public string ExactFurnaceLevelString {
        get {
            if(FurnaceLevel <= 29) {
                var clampedLevel = Math.Clamp(FurnaceLevel, 0, 30);
                return clampedLevel.ToString();
            }

            var rank = (FurnaceLevel - 30) / 5;
            var remainder = (FurnaceLevel - 30) % 5;

            if(rank == 0) {
                return $"30 {remainder}/5";
            }

            return $"FC{rank} {remainder}/5";
        }
    }
}

public class PlayerNameHistoryEntry {
    public required string Name { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
}

public class PlayerStateHistoryEntry {
    public required int State { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
}
