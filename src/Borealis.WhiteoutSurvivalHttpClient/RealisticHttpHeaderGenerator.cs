using System.Net.Http.Headers;

namespace Borealis.WhiteoutSurvivalHttpClient;

internal static class RealisticHttpHeaderGenerator {
    // Browser data taken from https://github.com/whiteout-project/Whiteout-Survival-Discord-Bot/blob/a06a7d04a5821f6635aed944e3b6358c740efa3e/src/functions/utility/apiClient.js
    // MIT License
    private static BrowserProfile[] BrowserProfiles { get; } = [
        new() {
            Browser = "Chrome",
            Versions = [124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135],
                Platforms = [
                    new() { Os = "Windows NT 10.0; Win64; x64", SecPlatform = "\"Windows\"" },
                    new() { Os = "Windows NT 11.0; Win64; x64", SecPlatform = "\"Windows\"" },
                    new() { Os = "Macintosh; Intel Mac OS X 10_15_7", SecPlatform = "\"macOS\"" },
                    new() { Os = "X11; Linux x86_64", SecPlatform = "\"Linux\"" }
            ],
            SecUaGenerator = (ver) => $"\"Not:A-Brand\";v=\"99\", \"Google Chrome\";v=\"{ver}\", \"Chromium\";v=\"{ver}\""
        },
        new() {
            Browser = "Brave",
            Versions = [132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145],
                Platforms = [
                    new() { Os = "Windows NT 10.0; Win64; x64", SecPlatform = "\"Windows\"" },
                    new() { Os = "Windows NT 11.0; Win64; x64", SecPlatform = "\"Windows\"" },
                    new() { Os = "Macintosh; Intel Mac OS X 10_15_7", SecPlatform = "\"macOS\"" }
            ],
            SecUaGenerator = (ver) => $"\"Not:A-Brand\";v=\"99\", \"Brave\";v=\"{ver}\", \"Chromium\";v=\"{ver}\""
        },
        new() {
            Browser = "Edge",
            Versions = [124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135],
                Platforms = [
                    new() { Os = "Windows NT 10.0; Win64; x64", SecPlatform = "\"Windows\"" },
                    new() { Os = "Windows NT 11.0; Win64; x64", SecPlatform = "\"Windows\"" },
                    new() { Os = "Macintosh; Intel Mac OS X 10_15_7", SecPlatform = "\"macOS\"" }
            ],
            SecUaGenerator = (ver) => $"\"Not A(B)rand\";v=\"8\", \"Chromium\";v=\"{ver}\", \"Microsoft Edge\";v=\"{ver}\""
        }
    ];

    private static readonly Random _random = new();

    public static HttpRequestHeaders GenerateHeaders(HttpRequestHeaders headers) {
        var profile = BrowserProfiles[_random.Next(BrowserProfiles.Length)];
        var version = profile.Versions[_random.Next(profile.Versions.Length)];
        var platform = profile.Platforms[_random.Next(profile.Platforms.Length)];

        headers.UserAgent.ParseAdd($"Mozilla/5.0 ({platform.Os}) AppleWebKit/537.36 (KHTML, like Gecko) {profile.Browser}/{version}.0.0.0 Safari/537.36");
        headers.Add("Accept", "application/json, text/plain, */*");
        headers.Add("Accept-Language", "en-US,en;q=0.7");
        headers.Add("Origin", "https://wos-giftcode.centurygame.com");
        headers.Add("Referer", "https://wos-giftcode.centurygame.com/");
        headers.Add("sec-ch-ua", profile.SecUaGenerator(version));
        headers.Add("sec-ch-ua-mobile", "?0");
        headers.Add("sec-ch-ua-platform", platform.SecPlatform);
        headers.Add("sec-fetch-dest", "empty");
        headers.Add("sec-fetch-mode", "cors");
        headers.Add("sec-fetch-site", "same-site");
        headers.Add("sec-gpc", "1");

        return headers;
    }
}

internal record BrowserProfile {
    public required string Browser { get; init; }
    public required int[] Versions { get; init; }
    public required BrowserPlatform[] Platforms { get; init; }
    public required Func<int, string> SecUaGenerator { get; init; }
}

internal record BrowserPlatform {
    public required string Os { get; init; }
    public required string SecPlatform { get; init; }
}
