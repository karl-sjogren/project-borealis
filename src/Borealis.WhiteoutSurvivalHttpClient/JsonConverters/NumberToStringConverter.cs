using System.Text.Json;
using System.Text.Json.Serialization;

namespace Borealis.WhiteoutSurvivalHttpClient.JsonConverters;

public class NumberToStringConverter : JsonConverter<string?> {
    public override bool CanConvert(Type typeToConvert) {
        return typeof(string) == typeToConvert;
    }

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if(reader.TokenType == JsonTokenType.Number) {
            return reader.TryGetInt64(out var l) ?
                l.ToString() :
                reader.GetDouble().ToString();
        }

        if(reader.TokenType == JsonTokenType.String) {
            return reader.GetString();
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) {
        if(value == null) {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value);
    }
}
