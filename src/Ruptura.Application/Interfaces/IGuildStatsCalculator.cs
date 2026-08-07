using Ruptura.Domain.Entities;
using Ruptura.Shared.Guilds;

namespace Ruptura.Application.Interfaces;

public interface IGuildStatsCalculator
{
    GuildDerivedStats Calculate(
        GuildSheetData data,
        IReadOnlyList<GuildBuilding> buildings,
        IReadOnlyList<GuildStaff> staff,
        int researchPoints,
        IReadOnlyDictionary<Guid, CatalogEntry> installationCatalog);
}
