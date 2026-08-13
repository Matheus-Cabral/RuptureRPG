using System.Text.Json;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Application.Services;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Bestiary;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Combat;
using Ruptura.Shared.Encounters;

namespace Ruptura.Infrastructure.Services;

// In-session combat tracker CRUD (GM-4). The live tracker state (round, turn cursor, combatant
// roster) is persisted as a typed CombatState blob scoped to one campaign. Campaign-ownership auth
// mirrors EncounterService/RewardService: a non-owned/missing campaign or a foreign session yields
// Combat.NotFound (existence hidden). StartFromEncounter is server-authoritative — creature PV is
// resolved from the bestiary (ICreatureService), party PV from each alive sheet
// (ICharacterSheetService). The whole-state PUT clamps PV, validates conditions against
// CombatReference and recomputes IsDefeated. Every read/update orders the roster via CombatOrder
// and carries the campaign's server-derived Pressure (never trusted from the client).
public class CombatService(
    ICombatSessionRepository combatRepo,
    ICampaignRepository campaignRepo,
    IEncounterService encounterService,
    ICreatureService creatureService,
    ICharacterSheetService characterSheetService,
    CombatOrder combatOrder) : ICombatService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private const int NameMax = 200;

    // ── Reads ──────────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<CombatSessionResponse>>> GetForCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<IEnumerable<CombatSessionResponse>>(ErrorCodes.Combat.NotFound);

        var sessions = await combatRepo.GetByCampaignAsync(campaignId, ct);
        var responses = sessions.Select(s => MapToResponse(s, campaign)).ToList();
        return Result.Success<IEnumerable<CombatSessionResponse>>(responses);
    }

    public async Task<Result<CombatSessionResponse>> GetByIdAsync(
        Guid gameMasterId, Guid campaignId, Guid id, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<CombatSessionResponse>(ErrorCodes.Combat.NotFound);

        var session = await combatRepo.GetByIdAsync(id, ct);
        if (session is null || session.CampaignId != campaignId)
            return Result.Failure<CombatSessionResponse>(ErrorCodes.Combat.NotFound);

        return Result.Success(MapToResponse(session, campaign));
    }

    // ── Writes ─────────────────────────────────────────────────────────────────

    public async Task<Result<CombatSessionResponse>> CreateAsync(
        Guid gameMasterId, Guid campaignId, CreateCombatSessionRequest request, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<CombatSessionResponse>(ErrorCodes.Combat.NotFound);

        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<CombatSessionResponse>(ErrorCodes.Combat.NameRequired);

        var session = new CombatSession
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            Name = TruncName(request.Name),
            DataJson = JsonSerializer.Serialize(new CombatState(), JsonOpts),
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await combatRepo.AddAsync(session, ct);
        await combatRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(session, campaign));
    }

    public async Task<Result<CombatSessionResponse>> StartFromEncounterAsync(
        Guid gameMasterId, Guid campaignId, StartFromEncounterRequest request, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<CombatSessionResponse>(ErrorCodes.Combat.NotFound);

        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<CombatSessionResponse>(ErrorCodes.Combat.NameRequired);

        // Resolve the encounter through its own service — a foreign/missing encounter fails there
        // (existence hidden) and maps to EncounterInvalid at this boundary.
        var encounterResult = await encounterService.GetByIdAsync(gameMasterId, campaignId, request.EncounterId, ct);
        if (encounterResult.IsFailure)
            return Result.Failure<CombatSessionResponse>(ErrorCodes.Combat.EncounterInvalid);
        var encounter = encounterResult.Value!;

        var combatants = new List<Combatant>();

        // Creatures — PV from the bestiary, NOT the encounter's Np. One combatant per unit.
        var creaturesResult = await creatureService.GetForGameMasterAsync(gameMasterId, ct);
        var bestiary = (creaturesResult.Value ?? Enumerable.Empty<CreatureResponse>())
            .GroupBy(c => c.Id)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var line in encounter.Creatures)
        {
            var pv = bestiary.TryGetValue(line.CreatureId, out var creature) ? creature.Data.Pv : 0;
            for (var n = 1; n <= Math.Max(1, line.Quantity); n++)
            {
                combatants.Add(new Combatant
                {
                    Id = Guid.NewGuid(),
                    Name = $"{line.CreatureName} #{n}",
                    Kind = "Creature",
                    SourceId = line.CreatureId,
                    Initiative = 0,
                    Percepcao = 0,
                    MaxPv = pv,
                    CurrentPv = pv,
                    Conditions = [],
                    IsDefeated = pv <= 0
                });
            }
        }

        // Alive party — PV from each sheet's DerivedStats.MaxHp / Data.Combat.CurrentHp.
        var sheetsResult = await characterSheetService.GetByCampaignAsync(gameMasterId, campaignId, ct);
        var alive = (sheetsResult.Value ?? Enumerable.Empty<CharacterSheetResponse>())
            .Where(s => !s.IsDead && !s.IsRetired);

        foreach (var sheet in alive)
        {
            var maxPv = Math.Max(0, sheet.DerivedStats.MaxHp);
            var currentPv = Math.Clamp(sheet.Data.Combat.CurrentHp, 0, maxPv);
            combatants.Add(new Combatant
            {
                Id = Guid.NewGuid(),
                Name = sheet.CharacterName,
                Kind = "Character",
                SourceId = sheet.Id,
                Initiative = 0,
                Percepcao = sheet.Data.Attributes.Percepcao,
                MaxPv = maxPv,
                CurrentPv = currentPv,
                Conditions = [],
                IsDefeated = currentPv <= 0
            });
        }

        var state = new CombatState { Round = 1, CurrentIndex = 0, Combatants = combatants };

        var session = new CombatSession
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            Name = TruncName(request.Name),
            DataJson = JsonSerializer.Serialize(state, JsonOpts),
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await combatRepo.AddAsync(session, ct);
        await combatRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(session, campaign));
    }

    public async Task<Result<CombatSessionResponse>> UpdateStateAsync(
        Guid gameMasterId, Guid campaignId, Guid sessionId, UpdateCombatStateRequest request, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<CombatSessionResponse>(ErrorCodes.Combat.NotFound);

        var session = await combatRepo.GetByIdAsync(sessionId, ct);
        if (session is null || session.CampaignId != campaignId)
            return Result.Failure<CombatSessionResponse>(ErrorCodes.Combat.NotFound);

        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<CombatSessionResponse>(ErrorCodes.Combat.NameRequired);

        // Reject a missing State instead of silently persisting an empty roster (data loss).
        if (request.State is null)
            return Result.Failure<CombatSessionResponse>(ErrorCodes.Combat.StateRequired);

        var incoming = request.State;

        // Structural validation + clamps: drop null combatants, floor MaxPv, clamp CurrentPv into
        // [0, MaxPv], validate every condition, recompute IsDefeated.
        var combatants = new List<Combatant>();
        foreach (var c in incoming.Combatants ?? [])
        {
            if (c is null) continue;

            var conditions = (c.Conditions ?? []).ToList();
            if (conditions.Any(cond => !CombatReference.Conditions.Contains(cond)))
                return Result.Failure<CombatSessionResponse>(ErrorCodes.Combat.ConditionInvalid);

            // De-dupe so a repeated condition (e.g. ["Morto","Morto"]) can't round-trip and
            // require two toggle clicks to clear.
            conditions = conditions.Distinct().ToList();

            var maxPv = Math.Max(0, c.MaxPv);
            var currentPv = Math.Clamp(c.CurrentPv, 0, maxPv);

            combatants.Add(new Combatant
            {
                Id = c.Id == Guid.Empty ? Guid.NewGuid() : c.Id,
                Name = c.Name,
                Kind = c.Kind,
                SourceId = c.SourceId,
                Initiative = c.Initiative,
                Percepcao = c.Percepcao,
                MaxPv = maxPv,
                CurrentPv = currentPv,
                Conditions = conditions,
                Notes = c.Notes,
                IsDefeated = currentPv <= 0 || conditions.Contains("Morto")
            });
        }

        var round = Math.Max(1, incoming.Round);
        var clampedIndex = combatants.Count == 0
            ? 0
            : Math.Clamp(incoming.CurrentIndex, 0, combatants.Count - 1);

        // Persist the roster in initiative order and follow the current combatant's identity
        // across the sort, so the "current turn" cursor never silently jumps when the incoming
        // list is stale w.r.t. the order (a GM edited an Initiative, added a reinforcement,
        // etc.). CombatOrder.Order is idempotent, so MapToResponse's re-sort becomes a no-op and
        // persisted order == response order permanently.
        var currentId = combatants.ElementAtOrDefault(clampedIndex)?.Id;
        combatants = combatOrder.Order(combatants).ToList();
        var newIndex = currentId is null ? 0 : combatants.FindIndex(c => c.Id == currentId.Value);
        if (newIndex < 0) newIndex = 0;

        var state = new CombatState { Round = round, CurrentIndex = newIndex, Combatants = combatants };

        session.Name = TruncName(request.Name);
        session.IsActive = request.IsActive;
        session.DataJson = JsonSerializer.Serialize(state, JsonOpts);
        session.UpdatedAt = DateTime.UtcNow;

        combatRepo.Update(session);
        await combatRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(session, campaign));
    }

    public async Task<Result> DeleteAsync(
        Guid gameMasterId, Guid campaignId, Guid id, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure(ErrorCodes.Combat.NotFound);

        var session = await combatRepo.GetByIdAsync(id, ct);
        if (session is null || session.CampaignId != campaignId)
            return Result.Failure(ErrorCodes.Combat.NotFound);

        combatRepo.Remove(session);
        await combatRepo.SaveChangesAsync(ct);
        return Result.Success();
    }

    // ── Auth ──────────────────────────────────────────────────────────────────

    private async Task<Campaign?> LoadOwnedCampaignAsync(Guid gameMasterId, Guid campaignId, CancellationToken ct)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        return campaign is not null && campaign.GameMasterId == gameMasterId ? campaign : null;
    }

    // ── Mapping ──────────────────────────────────────────────────────────────────

    private CombatSessionResponse MapToResponse(CombatSession session, Campaign campaign)
    {
        var state = Deserialize(session.DataJson);
        state.Combatants = combatOrder.Order(state.Combatants).ToList();

        return new CombatSessionResponse
        {
            Id = session.Id,
            Name = session.Name,
            IsActive = session.IsActive,
            State = state,
            Pressure = campaign.Pressure,
            PressureStateKey = DungeonPressure.StateFor(campaign.Pressure).StateKey
        };
    }

    private static CombatState Deserialize(string json)
    {
        CombatState? state;
        try { state = JsonSerializer.Deserialize<CombatState>(json, JsonOpts); }
        catch (JsonException) { state = null; }
        return state ?? new CombatState();
    }

    private static string TruncName(string? name)
    {
        var value = (name ?? string.Empty).Trim();
        return value.Length > NameMax ? value[..NameMax] : value;
    }
}
