using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Aptitudes =
    [
        Entry("40000000-0000-0000-0000-000000000001", CatalogEntryType.Aptitude, "Combate", new { CoveredAreas = new[] { "Combate — Armas", "Combate — Defesa", "Combate Corporal", "Combate à Distância" } }),
        Entry("40000000-0000-0000-0000-000000000002", CatalogEntryType.Aptitude, "Exploração", new { CoveredAreas = new[] { "Exploração" } }),
        Entry("40000000-0000-0000-0000-000000000003", CatalogEntryType.Aptitude, "Conhecimento", new { CoveredAreas = new[] { "Conhecimento", "Cura" } }),
        Entry("40000000-0000-0000-0000-000000000004", CatalogEntryType.Aptitude, "Ofício", new { CoveredAreas = new[] { "Artesanato", "Alquimia" } }),
        Entry("40000000-0000-0000-0000-000000000005", CatalogEntryType.Aptitude, "Magia", new { CoveredAreas = new[] { "Magia" } }),
        Entry("40000000-0000-0000-0000-000000000006", CatalogEntryType.Aptitude, "Liderança", new { CoveredAreas = new[] { "Social" } }),
    ];
}
