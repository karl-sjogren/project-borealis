using System.Text.Json.Serialization;

namespace Borealis.WhiteoutSurvivalHttpClient.Models;

public record WhiteoutSurvivalResponseWrapper {
    [JsonPropertyName("msg")]
    public required string Message { get; init; }

    [JsonPropertyName("code")]
    public int Code { get; init; } // 0 for success, 1 for failure?

    /// <summary>
    /// Fake property to handle the conversion of the error code that can be string or int. Do not use.
    /// </summary>
    [JsonPropertyName("err_code")]
    public object _errorCode {
        get {
            return ErrorCode;
        }
        set {
            _ = int.TryParse(value.ToString(), out var errorCode);
            ErrorCode = errorCode;
        }
    }

    /// <summary>
    /// Error code from API. Known values:
    /// - 0: Unknown or unsed
    /// - 20000: Code success
    /// - 40014: Code not found
    /// - 40004: Player not found
    /// - 40005: Claim limit reached
    /// - 40007: Code expired
    /// - 40008: Code already used
    /// - 40009: Not logged in
    /// - 40011: Currently unknown
    /// - 40100: Captcha requested too frequently
    /// - 40103: Captcha failed
    /// </summary>
    [JsonIgnore]
    public int ErrorCode { get; set; }
}

public record WhiteoutSurvivalResponseWrapper<TResponseType> : WhiteoutSurvivalResponseWrapper {
    public required TResponseType Data { get; init; }
}
