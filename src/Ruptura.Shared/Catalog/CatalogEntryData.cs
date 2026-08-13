using System.Text.Json;
using System.Text.Json.Nodes;

namespace Ruptura.Shared.Catalog;

// Pure, DI-free working model for a catalog entry's DataJson. The form edits known schema
// keys; any other keys are preserved verbatim (round-trip, no data loss).
public sealed class CatalogEntryData
{
    private readonly JsonObject _root;
    private CatalogEntryData(JsonObject root) => _root = root;

    public static CatalogEntryData Parse(string? dataJson)
    {
        if (!string.IsNullOrWhiteSpace(dataJson))
        {
            try
            {
                if (JsonNode.Parse(dataJson) is JsonObject obj)
                    return new CatalogEntryData(obj);
            }
            catch (JsonException) { /* fall through to empty */ }
        }
        return new CatalogEntryData(new JsonObject());
    }

    public static bool TryParse(string? raw, out CatalogEntryData data)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(raw) && JsonNode.Parse(raw) is JsonObject obj)
            {
                data = new CatalogEntryData(obj);
                return true;
            }
        }
        catch (JsonException) { /* invalid */ }
        data = new CatalogEntryData(new JsonObject());
        return false;
    }

    public string GetString(string key) =>
        _root.TryGetPropertyValue(key, out var n) && n is not null ? n.ToString() : string.Empty;

    public double? GetNumber(string key) =>
        _root.TryGetPropertyValue(key, out var n) && n is JsonValue v && v.TryGetValue<double>(out var d) ? d : null;

    public bool GetBool(string key) =>
        _root.TryGetPropertyValue(key, out var n) && n is JsonValue v && v.TryGetValue<bool>(out var b) && b;

    public void SetString(string key, string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed)) _root.Remove(key);
        else _root[key] = trimmed;
    }

    public void SetNumber(string key, double? value)
    {
        if (value is null) _root.Remove(key);
        else _root[key] = value.Value;
    }

    public void SetBool(string key, bool value) => _root[key] = value;

    public string ToJson(bool indented = false) =>
        _root.ToJsonString(new JsonSerializerOptions { WriteIndented = indented });
}
