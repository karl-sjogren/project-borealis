using System.Text.Json.Serialization;

namespace Borealis.Core.HttpClients.DataTransferObjects;

public class CapSolverImageToTextResponse {
    [JsonPropertyName("errorId")]
    public int ErrorId { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("solution")]
    public CapSolverSolution? Solution { get; set; }

    [JsonPropertyName("taskId")]
    public required string TaskId { get; set; }
}

public class CapSolverSolution {
    [JsonPropertyName("answers")]
    public string[]? Answers { get; set; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
