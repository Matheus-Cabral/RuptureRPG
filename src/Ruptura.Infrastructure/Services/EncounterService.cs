using System.Text.Json;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Application.Services;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Bestiary;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Encounters;

namespace Ruptura.Infrastructure.Services;

// Encounter CRUD + server-authoritative threat resolution (GDD §9.8/§9.9). Composes the
// existing party (ICharacterSheetService), bestiary (ICreatureService) and campaign Pressão
// (DungeonPressure) sources — never re-implementing that logic — and runs EncounterCalculator.
// Campaign-ownership auth mirrors CampaignDashboardService: a non-owned/missing campaign or a
// foreign encounter yields Encounter.NotFound (existence hidden).
public class EncounterService(
    IEncounterRepository encounterRepo,
    ICampaignRepository campaignRepo,
    ICharacterSheetService characterSheetService,
    ICreatureService creatureService,
    IEncounterCalculator calculator) : IEncounterService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private const int NameMax = 200;

    // Rank → FCE band. The four bands are the exact EncounterReference.FceByRankingBand keys.
    private static readonly IReadOnlyDictionary<string, string> BandByRank = new Dictionary<string, string>
    {
        ["Bronze"] = "BronzeFerro",
        ["Ferro"] = "BronzeFerro",
        ["Aço"] = "AcoPrata",
        ["Prata"] = "AcoPrata",
        ["Ouro"] = "OuroMithril",
        ["Mithril"] = "OuroMithril",
        ["Adamante"] = "AdamanteLendario",
        ["Lendário"] = "AdamanteLendario",
    };

    // Ascending band strength — used to break a dominant-band tie toward the stronger band.
    private static readonly string[] BandOrder =
        ["BronzeFerro", "AcoPrata", "OuroMithril", "AdamanteLendario"];

    // No alive party → the most conservative (highest-FCE) band.
    private const string DefaultBand = "BronzeFerro";

    // ── Reads ──────────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<EncounterResponse>>> GetForCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<IEnumerable<EncounterResponse>>(ErrorCodes.Encounter.NotFound);

        var context = await BuildResolutionContextAsync(campaign, gameMasterId, ct);
        var encounters = await encounterRepo.GetByCampaignAsync(campaignId, ct);
        var responses = encounters.Select(e => MapToResponse(e, context)).ToList();
        return Result.Success<IEnumerable<EncounterResponse>>(responses);
    }

    public async Task<Result<EncounterResponse>> GetByIdAsync(
        Guid gameMasterId, Guid campaignId, Guid id, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<EncounterResponse>(ErrorCodes.Encounter.NotFound);

        var encounter = await encounterRepo.GetByIdAsync(id, ct);
        if (encounter is null || encounter.CampaignId != campaignId)
            return Result.Failure<EncounterResponse>(ErrorCodes.Encounter.NotFound);

        var context = await BuildResolutionContextAsync(campaign, gameMasterId, ct);
        return Result.Success(MapToResponse(encounter, context));
    }

    // ── Writes ─────────────────────────────────────────────────────────────────

    public async Task<Result<EncounterResponse>> CreateAsync(
        Guid gameMasterId, Guid campaignId, CreateEncounterRequest request, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<EncounterResponse>(ErrorCodes.Encounter.NotFound);

        var data = request.Data ?? new EncounterData();
        var validationError = Validate(request.Name, data);
        if (validationError is not null)
            return Result.Failure<EncounterResponse>(validationError);

        Sanitize(data);

        var encounter = new Encounter
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            Name = TruncName(request.Name),
            DataJson = JsonSerializer.Serialize(data, JsonOpts),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await encounterRepo.AddAsync(encounter, ct);
        await encounterRepo.SaveChangesAsync(ct);

        var context = await BuildResolutionContextAsync(campaign, gameMasterId, ct);
        return Result.Success(MapToResponse(encounter, context));
    }

    public async Task<Result<EncounterResponse>> UpdateAsync(
        Guid gameMasterId, Guid campaignId, Guid id, UpdateEncounterRequest request, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<EncounterResponse>(ErrorCodes.Encounter.NotFound);

        var encounter = await encounterRepo.GetByIdAsync(id, ct);
        if (encounter is null || encounter.CampaignId != campaignId)
            return Result.Failure<EncounterResponse>(ErrorCodes.Encounter.NotFound);

        var data = request.Data ?? new EncounterData();
        var validationError = Validate(request.Name, data);
        if (validationError is not null)
            return Result.Failure<EncounterResponse>(validationError);

        Sanitize(data);

        encounter.Name = TruncName(request.Name);
        encounter.DataJson = JsonSerializer.Serialize(data, JsonOpts);
        encounter.UpdatedAt = DateTime.UtcNow;

        encounterRepo.Update(encounter);
        await encounterRepo.SaveChangesAsync(ct);

        var context = await BuildResolutionContextAsync(campaign, gameMasterId, ct);
        return Result.Success(MapToResponse(encounter, context));
    }

    public async Task<Result> DeleteAsync(
        Guid gameMasterId, Guid campaignId, Guid id, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure(ErrorCodes.Encounter.NotFound);

        var encounter = await encounterRepo.GetByIdAsync(id, ct);
        if (encounter is null || encounter.CampaignId != campaignId)
            return Result.Failure(ErrorCodes.Encounter.NotFound);

        encounterRepo.Remove(encounter);
        await encounterRepo.SaveChangesAsync(ct);
        return Result.Success();
    }

    // ── Auth ──────────────────────────────────────────────────────────────────

    private async Task<Campaign?> LoadOwnedCampaignAsync(Guid gameMasterId, Guid campaignId, CancellationToken ct)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        return campaign is not null && campaign.GameMasterId == gameMasterId ? campaign : null;
    }

    // ── Validation & sanitizing ─────────────────────────────────────────────────

    // Name required + fixed-set validation for the five picker fields. Unknown creature IDs are
    // NOT rejected here (they resolve to Np 0, flagged) — the bestiary is the source of truth.
    private static string? Validate(string? name, EncounterData data)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ErrorCodes.Encounter.NameRequired;
        if (!EncounterReference.Intelligences.Contains(data.Intelligence))
            return ErrorCodes.Encounter.IntelligenceInvalid;
        if (!EncounterReference.Terrains.Contains(data.Terrain))
            return ErrorCodes.Encounter.TerrainInvalid;
        if (!EncounterReference.Objectives.Contains(data.Objective))
            return ErrorCodes.Encounter.ObjectiveInvalid;
        if (!EncounterReference.Difficulties.Contains(data.DesiredDifficulty))
            return ErrorCodes.Encounter.DifficultyInvalid;
        if (!EncounterReference.Durations.Contains(data.Duration))
            return ErrorCodes.Encounter.DurationInvalid;
        return null;
    }

    // Upper bound on a single creature line's quantity — high enough for any sane encounter,
    // low enough that quantity × Np can never overflow the calculator's long math.
    private const int QuantityMax = 9999;

    // Cap on the number of distinct creature lines persisted in one encounter. Bounds DataJson
    // size and the calculator's per-line work for an authenticated-but-hostile GM.
    private const int MaxCreatureLines = 200;

    // Defense-in-depth: drop null creature lines, clamp each quantity to [1, 9999] so a stray
    // 0/negative can't silently drop a line or subtract from PE and a huge value can't overflow
    // the calculator, and cap the list to the first 200 lines. Overrides floored at ≥0.
    private static void Sanitize(EncounterData data)
    {
        data.Creatures = (data.Creatures ?? [])
            .Where(c => c is not null)
            .Take(MaxCreatureLines)
            .Select(c => { c.Quantity = Math.Clamp(c.Quantity, 1, QuantityMax); return c; })
            .ToList();

        if (data.PartyNpOverride is { } np) data.PartyNpOverride = Math.Max(0, np);
        if (data.PartySizeOverride is { } size) data.PartySizeOverride = Math.Max(1, size);
    }

    private static string TruncName(string? name)
    {
        var value = (name ?? string.Empty).Trim();
        return value.Length > NameMax ? value[..NameMax] : value;
    }

    // ── Resolution ──────────────────────────────────────────────────────────────

    // Everything the mapper needs that is shared across all encounters in a campaign: the alive
    // party's NPs + FCE band, the campaign Pressão multiplier, and the GM's visible bestiary.
    private sealed record ResolutionContext(
        IReadOnlyList<int> AlivePartyNps,
        string PartyBand,
        int PressureValue,
        decimal PressureMultiplier,
        IReadOnlyDictionary<Guid, (string Name, int Np)> Bestiary);

    private async Task<ResolutionContext> BuildResolutionContextAsync(
        Campaign campaign, Guid gameMasterId, CancellationToken ct)
    {
        // Party — alive, non-retired (mirrors CampaignDashboardService).
        var sheetsResult = await characterSheetService.GetByCampaignAsync(gameMasterId, campaign.Id, ct);
        var alive = (sheetsResult.Value ?? Enumerable.Empty<CharacterSheetResponse>())
            .Where(s => !s.IsDead && !s.IsRetired)
            .ToList();

        var partyNps = alive.Select(s => s.DerivedStats.Np).ToList();
        var band = ResolveBand(alive.Select(s => s.Data.GuildRegistry.Ranking));

        var (_, pressureMult) = DungeonPressure.StateFor(campaign.Pressure);

        // Bestiary — own homebrew + official examples, keyed for O(1) lookup per creature line.
        var creaturesResult = await creatureService.GetForGameMasterAsync(gameMasterId, ct);
        var bestiary = (creaturesResult.Value ?? Enumerable.Empty<CreatureResponse>())
            .GroupBy(c => c.Id)
            .ToDictionary(g => g.Key, g => (g.First().Name, g.First().DerivedNp));

        return new ResolutionContext(partyNps, band, campaign.Pressure, pressureMult, bestiary);
    }

    // Dominant-band rule: the band shared by the most alive members wins; a tie breaks toward the
    // STRONGER band (later in BandOrder). No alive party → DefaultBand (BronzeFerro).
    private static string ResolveBand(IEnumerable<string> rankings)
    {
        var bands = rankings
            .Select(r => BandByRank.GetValueOrDefault(r ?? string.Empty, DefaultBand))
            .ToList();
        if (bands.Count == 0) return DefaultBand;

        return bands
            .GroupBy(b => b)
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => Array.IndexOf(BandOrder, g.Key))
            .First().Key;
    }

    // ── Mapping ──────────────────────────────────────────────────────────────────

    private EncounterResponse MapToResponse(Encounter encounter, ResolutionContext context)
    {
        var data = Deserialize(encounter.DataJson);

        // Resolve each creature line against the GM's bestiary. A missing/invisible creature
        // contributes Np 0 with an empty name — listed (flagged), never thrown.
        var resolved = (data.Creatures ?? [])
            .Where(c => c is not null)
            .Select(c =>
            {
                var found = context.Bestiary.TryGetValue(c.CreatureId, out var hit);
                return new EncounterCreatureResolved
                {
                    CreatureId = c.CreatureId,
                    CreatureName = found ? hit.Name : string.Empty,
                    Np = found ? hit.Np : 0,
                    Quantity = Math.Max(1, c.Quantity)
                };
            })
            .ToList();

        // PG source: a PartyNpOverride bypasses the alive party entirely; PartySizeOverride (if
        // set) drives the §9.8 synergy tier, else the alive-party count.
        var hasOverride = data.PartyNpOverride.HasValue;
        var partyNps = hasOverride
            ? new List<int> { Math.Max(0, data.PartyNpOverride!.Value) }
            : context.AlivePartyNps.ToList();
        var partySize = data.PartySizeOverride
            ?? (hasOverride ? 1 : context.AlivePartyNps.Count);

        var applyPressure = data.ApplyPressure;
        var pressureMult = applyPressure ? context.PressureMultiplier : 1.0m;

        var threat = calculator.Calculate(
            partyNps,
            partySize,
            resolved.Select(r => (r.Np, r.Quantity)),
            data.Intelligence,
            data.Terrain,
            data.Objective,
            pressureMult,
            data.DesiredDifficulty,
            data.Duration,
            context.PartyBand);

        return new EncounterResponse
        {
            Id = encounter.Id,
            CampaignId = encounter.CampaignId,
            Name = encounter.Name,
            Intelligence = data.Intelligence,
            Terrain = data.Terrain,
            Objective = data.Objective,
            ApplyPressure = applyPressure,
            Floor = data.Floor,
            PartyNpOverride = data.PartyNpOverride,
            PartySizeOverride = data.PartySizeOverride,
            DesiredDifficulty = data.DesiredDifficulty,
            Duration = data.Duration,
            Creatures = resolved,
            Pg = threat.Pg,
            Pe = threat.Pe,
            R = threat.R,
            RLabel = threat.RLabel,
            Oa = threat.Oa,
            Fce = threat.Fce,
            RealStatMultiplier = threat.RealStatMultiplier,
            PressureApplied = applyPressure,
            PressureValue = applyPressure ? context.PressureValue : 0,
            // Consistent with the calculator contract: a verdict exists only when PG resolved (>0).
            // A party of all-NP-0 characters, or a PartyNpOverride of 0, yields no verdict.
            PartyResolved = threat.Pg > 0,
            CreatedAt = encounter.CreatedAt,
            UpdatedAt = encounter.UpdatedAt
        };
    }

    // A single corrupted/hand-edited DB blob must degrade to a default object, never 500 the
    // whole list endpoint. Besides malformed JSON (JsonException), a type mismatch on a valid
    // JSON shape can surface as NotSupportedException — catch both.
    private static EncounterData Deserialize(string json)
    {
        EncounterData? data;
        try { data = JsonSerializer.Deserialize<EncounterData>(json, JsonOpts); }
        catch (Exception ex) when (ex is JsonException or NotSupportedException) { data = null; }
        return data ?? new EncounterData();
    }
}
