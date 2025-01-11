using System.Text.Json.Serialization;

namespace Borealis.Core.HttpClients.Models;

public record WhiteoutSurvivalPlayerResponse {
    [JsonPropertyName("Fid")]
    public required int FurnaceId { get; init; }

    [JsonPropertyName("Kid")]
    public required int State { get; init; }

    [JsonPropertyName("Nickname")]
    public required string Name { get; init; }

    [JsonPropertyName("Stove_lv")]
    public int FurnaceLevel { get; init; }

    [JsonPropertyName("Stove_lv_content")]
    public string? FurnaceLevelBadge { get; init; }

    [JsonPropertyName("Total_recharge_amount")]
    public int TotalRechargeAmount { get; init; }
}
