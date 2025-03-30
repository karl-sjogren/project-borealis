using System.Text.Json;
using System.Text.Json.Serialization;
using Borealis.WhiteoutSurvivalHttpClient.Models;

namespace Borealis.WhiteoutSurvivalHttpClient.JsonConverters;

internal class WhiteoutSurvivalPlayerResponseJsonConverter : JsonConverter<WhiteoutSurvivalPlayerResponse?> {
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public override void Write(Utf8JsonWriter writer, WhiteoutSurvivalPlayerResponse? value, JsonSerializerOptions options) {
        throw new NotImplementedException();
    }

    public override WhiteoutSurvivalPlayerResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if(reader.TokenType == JsonTokenType.Null) {
            return null;
        }

        if(reader.TokenType == JsonTokenType.StartArray) {
            while(reader.Read()) {
                if(reader.TokenType == JsonTokenType.EndArray) {
                    break;
                }
            }

            return null;
        }

        if(reader.TokenType == JsonTokenType.StartObject) {
            return JsonSerializer.Deserialize<WhiteoutSurvivalPlayerResponse?>(ref reader, _serializerOptions);
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}.");
    }
}
