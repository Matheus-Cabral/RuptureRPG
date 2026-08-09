using System.Text.Json;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Guilds;

namespace Ruptura.Application.Services;

public class GuildStatsCalculator : IGuildStatsCalculator
{
    private static readonly decimal[] InflationByStage =
        [1.0m, 1.2m, 1.5m, 1.8m, 2.2m, 2.6m, 3.2m, 4.0m];

    public GuildDerivedStats Calculate(
        GuildSheetData data,
        IReadOnlyList<GuildBuilding> buildings,
        IReadOnlyList<GuildStaff> staff,
        int researchPoints,
        IReadOnlyDictionary<Guid, CatalogEntry> installationCatalog)
    {
        var resources = data.Resources ?? new GuildResources();
        var activeDoctrines = data.ActiveDoctrineIds ?? [];
        var hasLogistica = activeDoctrines.Contains(GuildCatalogIds.DoctrineLogistica);
        var hasComercial = activeDoctrines.Contains(GuildCatalogIds.DoctrineComercial);

        // Per-installation level (0 if not built OR inactive). Inactive buildings grant no functional
        // benefit (GDD §10.9 "inativa = sem benefício"), so they are excluded from every LevelOf-derived
        // stat (CS/CI/CF, StorageCapacity, ResidencyCapacity, DoctrineLimit). Unique index guarantees at
        // most one row per installation.
        int LevelOf(Guid installationId) =>
            buildings.FirstOrDefault(b => b.CatalogEntryId == installationId && b.IsActive)?.Level ?? 0;

        var armazem = LevelOf(GuildCatalogIds.Armazem);
        var centroLog = LevelOf(GuildCatalogIds.CentroLogistico);
        var camara = LevelOf(GuildCatalogIds.CamaraDoConselho);
        var memorial = LevelOf(GuildCatalogIds.Memorial);
        var biblioteca = LevelOf(GuildCatalogIds.Biblioteca);
        var campo = LevelOf(GuildCatalogIds.CampoDeTreinamento);
        var dormitorio = LevelOf(GuildCatalogIds.Dormitorio);

        // CS/CI/CF (§10.9). CS gets the Logística +20% (floored).
        var baseCs = 5 + centroLog * 2 + armazem * 1;
        var cs = hasLogistica ? (int)Math.Floor(baseCs * 1.20m) : baseCs;
        var ci = 3 + camara * 4 + centroLog * 1;
        var cf = 10 + memorial * 3 + biblioteca * 1 + campo * 1;

        // Constructible buildings only (exclude Portão / NonConstructible) for Infra, maintenance, CS cap.
        // Unknown/malformed catalog entries (Data is null) are skipped — no weight means nothing to count.
        // NOTE: Infra (CG) and daily maintenance intentionally sum over ALL constructible buildings,
        // active or not — deactivation removes functional benefits (LevelOf) but not upkeep cost.
        var constructible = buildings
            .Select(b => (Building: b, Data: DataFor(b.CatalogEntryId, installationCatalog)))
            .Where(x => x.Data is not null && !x.Data.NonConstructible)
            .ToList();

        var infra = constructible.Sum(x => x.Building.Level * x.Data!.Weight);

        var activeWorkers = staff.Count(s => s.Kind == GuildStaffKind.Worker && s.IsActive); // all workers qualify
        var logistica = cs + activeWorkers * 2;

        // §10.8 Recursos = Moedas de Pacto (face value) + materiais estratégicos (VE 0..5).
        // Raw Quantity and DimensionalFragments deliberately excluded (closes guild-sheet spec §11.3):
        // Quantity is inventory only; Fragments are the separate RE pillar. long-sum + clamp keeps
        // a legacy/hand-edited blob from overflowing the guild read.
        var recursos = ClampToInt(
            (long)resources.PactCoins
            + (resources.Materials ?? []).Sum(m => (long)Math.Clamp(m.StrategicValue, 0, 5)));

        // The four CG terms are each bounded ints, but their sum can still exceed int range — add in long.
        var cg = ClampToInt((long)infra + researchPoints + logistica + recursos);

        // Daily maintenance: constructible buildings (level×weight) + active staff salaries; Logística −10%.
        var buildingMaintenance = constructible.Sum(x => x.Building.Level * x.Data!.Weight); // 1 Prata per level×weight
        var salaries = staff.Where(s => s.IsActive).Sum(s => s.DailySalary);
        var baseMaintenance = buildingMaintenance + salaries;
        var maintenance = hasLogistica
            ? (int)Math.Round(baseMaintenance * 0.90m, MidpointRounding.AwayFromZero)
            : baseMaintenance;

        var workerIncome = staff.Count(s => s.Kind == GuildStaffKind.Worker && s.IsActive && s.TypeOrRanking == GuildStaffTypes.Operario) * 2;

        var stageIndex = StageIndex(data.FloorsConquered);
        var inflationStageIndex = hasComercial ? Math.Max(0, stageIndex - 1) : stageIndex;

        var activeBuildingCount = constructible.Count(x => x.Building.IsActive);

        return new GuildDerivedStats
        {
            Stage = (GuildStage)stageIndex,
            StageIndex = stageIndex,
            Cg = cg,
            CgInfra = infra,
            CgPesquisa = researchPoints,
            CgLogistica = logistica,
            CgRecursos = recursos,
            Cs = cs,
            Ci = ci,
            Cf = cf,
            InflationIndex = InflationByStage[inflationStageIndex],
            DailyMaintenance = maintenance,
            WorkerIncomePerDay = workerIncome,
            StorageCapacity = armazem * 50,
            ResidencyCapacity = dormitorio * 2,
            DoctrineLimit = Math.Min(4, 2 + camara),
            ActiveDoctrineCount = activeDoctrines.Count,
            ActiveDoctrineOverflow = activeDoctrines.Count > Math.Min(4, 2 + camara),
            ActiveBuildingCount = activeBuildingCount,
            ActiveBuildingOverflow = activeBuildingCount > cs
        };
    }

    // Saturating narrowing: keeps aggregate sums from throwing OverflowException on extreme stored data.
    private static int ClampToInt(long v) => (int)Math.Clamp(v, int.MinValue, int.MaxValue);

    private static int StageIndex(int floors) => floors switch
    {
        >= 35 => 7,
        >= 30 => 6,
        >= 25 => 5,
        >= 20 => 4,
        >= 15 => 3,
        >= 10 => 2,
        >= 5 => 1,
        _ => 0
    };

    // Malformed homebrew installation DataJson must never 500 a guild read — treat as null (skipped).
    private static InstallationCatalogData? DataFor(Guid id, IReadOnlyDictionary<Guid, CatalogEntry> catalog)
    {
        if (!catalog.TryGetValue(id, out var entry)) return null;
        try { return JsonSerializer.Deserialize<InstallationCatalogData>(entry.DataJson); }
        catch (JsonException) { return null; }
    }
}
