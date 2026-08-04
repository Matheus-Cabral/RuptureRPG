using System.Text.Json;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    // Fixed timestamp so HasData produces a stable migration diff — using
    // DateTime.UtcNow here would make every migration regeneration look
    // like every seed row changed.
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static CatalogEntry Entry(string id, CatalogEntryType type, string name, object data) => new()
    {
        Id = Guid.Parse(id),
        Type = type,
        CampaignId = null,
        Name = name,
        DataJson = JsonSerializer.Serialize(data),
        CreatedByGameMasterId = null,
        CreatedAt = SeedTimestamp,
        UpdatedAt = SeedTimestamp
    };
}
