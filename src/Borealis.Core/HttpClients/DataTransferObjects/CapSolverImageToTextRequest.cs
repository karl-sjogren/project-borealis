using System.Text.Json.Serialization;

namespace Borealis.Core.HttpClients.DataTransferObjects;

public record CapSolverImageToTextRequest {
    [JsonPropertyName("clientKey")]
    public required string ClientKey { get; set; }

    [JsonPropertyName("task")]
    public required CapSolverImageToTextTask Task { get; set; }
}

public record CapSolverImageToTextTask {
    [JsonPropertyName("type")]
    public string Type { get; } = "ImageToTextTask";

    [JsonPropertyName("body")]
    public required string Body { get; set; }

    [JsonPropertyName("module")]
    public string? Module { get; set; }

    [JsonPropertyName("score")]
    public float? Score { get; set; }
}
