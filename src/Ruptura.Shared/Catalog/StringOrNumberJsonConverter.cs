using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ruptura.Shared.Catalog;

// GM Catalog admin form declares fields like Spell.ComplexityPaCost / Technique.PaCost as
// free Text (CatalogSchema.cs), but CatalogSeedData.Spells.cs / .Techniques.cs originally
// wrote them as JSON *numbers* (e.g. `ComplexityPaCost = 1`). CatalogEntryData.SetString only
// rewrites the JSON type of a key the GM actually edits through the structured form — any
// field the GM never touches keeps its original JSON type forever. JsonSerializer.Deserialize
// throws when a JSON number lands on a plain C# string property, and SafeDeserialize
// (CharacterStatsCalculator) swallows that into a null — silently zeroing out every other
// field on the same object, including PowerTier's NP contribution, even though PowerTier
// itself was saved correctly. Applying this converter to those fields lets either JSON shape
// deserialize instead of taking the whole object down with it.
internal sealed class StringOrNumberJsonConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.GetDouble().ToString(CultureInfo.InvariantCulture),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unexpected token {reader.TokenType} for a string/number field.")
        };

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
