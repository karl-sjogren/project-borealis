namespace Borealis.Core.Models;

public class WhiteoutSurvivalPlayer : EntityBase {
    public required int ExternalId { get; set; }
    public required string Name { get; set; }
    public required int State { get; set; }
    public required int FurnaceLevel { get; set; }
    public required bool IsInAlliance { get; set; }
    public string? Notes { get; set; }
    public IList<WhiteoutSurvivalPlayerNameHistoryEntry> PreviousNames { get; set; } = [];
}

public class WhiteoutSurvivalPlayerNameHistoryEntry {
    public required string Name { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
}
