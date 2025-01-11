namespace Borealis.Core.Models;

public class WhiteoutSurvivalPlayer : EntityBase {
    public required int ExternalId { get; set; }
    public required string Name { get; set; }
    public required int State { get; set; }
    public required int FurnaceLevel { get; set; }
    public required bool IsInAlliance { get; set; }
    public string? Notes { get; set; }
    public IList<WhiteoutSurvivalPlayerNameHistoryEntry> PreviousNames { get; set; } = [];

    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

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
}

public class WhiteoutSurvivalPlayerNameHistoryEntry {
    public required string Name { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
}
