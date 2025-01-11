using System.Text.RegularExpressions;

namespace Borealis.Web.Mvc;

public sealed partial class SlugifyParameterTransformer : IOutboundParameterTransformer {
    public string? TransformOutbound(object? value) {
        if(value == null) {
            return null;
        }

        var str = value.ToString();
        if(string.IsNullOrEmpty(str)) {
            return null;
        }

        return SlugRegex().Replace(str, "$1-$2").ToLowerInvariant();
    }

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex SlugRegex();
}
