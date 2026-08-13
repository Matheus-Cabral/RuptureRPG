using System.Text.Json;

namespace Ruptura.Shared.Content;

// Single source of truth for parsing a Floor's DataJson blob into a FloorData.
// Shared by CampaignContentService and CampaignDashboardService so the JSON options,
// the JsonException→empty guard, and the null-collection coalescing stay identical.
public static class FloorDataSerializer
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public static FloorData DeserializeFloor(string? json)
    {
        FloorData? data;
        try { data = string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<FloorData>(json, JsonOpts); }
        catch (JsonException) { data = null; }
        data ??= new FloorData();
        // Coalesce nullable collections: a hand-edited/corrupted DataJson can carry an explicit
        // "linkedEncounterIds": null that survives deserialization and would NRE downstream.
        data.LinkedEncounterIds ??= [];
        data.LinkedRewardIds ??= [];
        data.SecondaryObjectives ??= [];
        return data;
    }
}
