using System.Text.Json.Serialization;

namespace Borealis.WhiteoutSurvivalHttpClient.Models;

public record WhiteoutSurvivalPlayerResponse {
    [JsonPropertyName("Fid")]
    public required int FurnaceId { get; init; }

    [JsonPropertyName("Kid")]
    public required int State { get; init; }

    [JsonPropertyName("Nickname")]
    public required string Name { get; init; }

    [JsonPropertyName("avatar_image")]
    public string? AvatarUrl { get; init; }

    [JsonPropertyName("Stove_lv")]
    public int FurnaceLevel { get; init; }

    [JsonPropertyName("Total_recharge_amount")]
    public int TotalRechargeAmount { get; init; }
}
