using System.Security.Cryptography;
using System.Text;

namespace Borealis.WhiteoutSurvivalHttpClient.Common;

internal class WhiteoutSurvivalEncodedRequestContent : FormUrlEncodedContent {
    public WhiteoutSurvivalEncodedRequestContent(string secret, IEnumerable<KeyValuePair<string, string>> nameValueCollection) : base(GetEncodedValues(secret, nameValueCollection)) {
    }

    public static IEnumerable<KeyValuePair<string, string>> GetEncodedValues(string secret, IEnumerable<KeyValuePair<string, string>> nameValueCollection) {
        var sorted = nameValueCollection.OrderBy(x => x.Key);

        var stringToEncode = string.Join("&", sorted.Select(x => $"{x.Key}={x.Value}"));

        var hash = MD5.HashData(Encoding.UTF8.GetBytes(stringToEncode + secret));
        var hashString = Convert.ToHexStringLower(hash);

        return new[] { new KeyValuePair<string, string>("sign", hashString) }.Concat(sorted);
    }
}
