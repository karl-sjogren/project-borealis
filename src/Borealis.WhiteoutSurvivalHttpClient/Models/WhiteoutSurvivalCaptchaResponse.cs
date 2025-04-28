using System.Text.Json.Serialization;

namespace Borealis.WhiteoutSurvivalHttpClient.Models;

public record WhiteoutSurvivalCaptchaResponse {
    [JsonPropertyName("img")]
    public required string ImageData { get; init; }
}
