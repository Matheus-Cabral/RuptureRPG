using System.Text.Json;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Content;

namespace Ruptura.Infrastructure.Services;

// Session-prep content CRUD (GM-5): arcs and floors, each persisted as a typed DataJson blob scoped
// to one campaign. Campaign-ownership auth mirrors EncounterService/RewardService: a non-owned/missing
// campaign, or a foreign arc/floor, yields Content.NotFound (existence hidden — checked FIRST in every
// method). Arc requires a Name; floors do NOT (a blank floor name is allowed). Floors validate their
// ArcId (must belong to this campaign → Content.ArcInvalid), ObjectiveType (fixed set →
// Content.ObjectiveTypeInvalid), and every soft link (each LinkedEncounterId/LinkedRewardId must be an
// entity of this campaign → Content.LinkInvalid); link names are resolved at read time from the
// campaign's encounters/rewards. Deleting an arc cascades its floors at the DB level (DECISION D1).
public class CampaignContentService(
    IArcRepository arcRepo,
    IFloorRepository floorRepo,
    ICampaignRepository campaignRepo,
    IEncounterRepository encounterRepo,
    IRewardRepository rewardRepo) : ICampaignContentService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private const int NameMax = 200;

    // ── Arc reads ────────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<ArcResponse>>> GetArcsForCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<IEnumerable<ArcResponse>>(ErrorCodes.Content.NotFound);

        var arcs = await arcRepo.GetByCampaignAsync(campaignId, ct);
        return Result.Success<IEnumerable<ArcResponse>>(arcs.Select(MapArc).ToList());
    }

    public async Task<Result<ArcResponse>> GetArcByIdAsync(
        Guid gameMasterId, Guid campaignId, Guid arcId, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<ArcResponse>(ErrorCodes.Content.NotFound);

        var arc = await arcRepo.GetByIdAsync(arcId, ct);
        if (arc is null || arc.CampaignId != campaignId)
            return Result.Failure<ArcResponse>(ErrorCodes.Content.NotFound);

        return Result.Success(MapArc(arc));
    }

    // ── Arc writes ───────────────────────────────────────────────────────────────

    public async Task<Result<ArcResponse>> CreateArcAsync(
        Guid gameMasterId, Guid campaignId, CreateArcRequest request, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<ArcResponse>(ErrorCodes.Content.NotFound);

        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<ArcResponse>(ErrorCodes.Content.NameRequired);

        var data = request.Data ?? new ArcData();

        var arc = new Arc
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            Name = TruncName(request.Name),
            Order = request.Order,
            DataJson = JsonSerializer.Serialize(data, JsonOpts),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await arcRepo.AddAsync(arc, ct);
        await arcRepo.SaveChangesAsync(ct);

        return Result.Success(MapArc(arc));
    }

    public async Task<Result<ArcResponse>> UpdateArcAsync(
        Guid gameMasterId, Guid campaignId, Guid arcId, UpdateArcRequest request, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<ArcResponse>(ErrorCodes.Content.NotFound);

        var arc = await arcRepo.GetByIdAsync(arcId, ct);
        if (arc is null || arc.CampaignId != campaignId)
            return Result.Failure<ArcResponse>(ErrorCodes.Content.NotFound);

        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure<ArcResponse>(ErrorCodes.Content.NameRequired);

        var data = request.Data ?? new ArcData();

        arc.Name = TruncName(request.Name);
        arc.Order = request.Order;
        arc.DataJson = JsonSerializer.Serialize(data, JsonOpts);
        arc.UpdatedAt = DateTime.UtcNow;

        arcRepo.Update(arc);
        await arcRepo.SaveChangesAsync(ct);

        return Result.Success(MapArc(arc));
    }

    // Deleting the Arc entity cascades its Floors at the DB level (Floor→Arc FK ON DELETE CASCADE,
    // DECISION D1) — no manual floor removal needed.
    public async Task<Result> DeleteArcAsync(
        Guid gameMasterId, Guid campaignId, Guid arcId, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure(ErrorCodes.Content.NotFound);

        var arc = await arcRepo.GetByIdAsync(arcId, ct);
        if (arc is null || arc.CampaignId != campaignId)
            return Result.Failure(ErrorCodes.Content.NotFound);

        // Arc-delete cascades its floors at the DB level (DECISION D1). Capture this arc's floor ids
        // BEFORE the cascade removes them so a dangling CurrentFloorId pointer can be cleared below.
        var arcFloorIds = (await floorRepo.GetByArcAsync(arcId, ct)).Select(f => f.Id).ToList();

        arcRepo.Remove(arc);
        await arcRepo.SaveChangesAsync(ct);

        // Belt-and-suspenders (reads already degrade a missing floor to null in BuildAsync): if the
        // campaign's current-floor pointer targeted a cascaded floor, null it so the dashboard drops
        // the dangling reference.
        if (campaign.CurrentFloorId is { } cf && arcFloorIds.Contains(cf))
        {
            campaign.CurrentFloorId = null;
            campaignRepo.Update(campaign);
            await campaignRepo.SaveChangesAsync(ct);
        }

        return Result.Success();
    }

    // ── Floor reads ──────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<FloorResponse>>> GetFloorsForArcAsync(
        Guid gameMasterId, Guid campaignId, Guid arcId, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<IEnumerable<FloorResponse>>(ErrorCodes.Content.NotFound);

        var arc = await arcRepo.GetByIdAsync(arcId, ct);
        if (arc is null || arc.CampaignId != campaignId)
            return Result.Failure<IEnumerable<FloorResponse>>(ErrorCodes.Content.NotFound);

        var links = await BuildLinkContextAsync(campaignId, ct);
        var floors = await floorRepo.GetByArcAsync(arcId, ct);
        return Result.Success<IEnumerable<FloorResponse>>(floors.Select(f => MapFloor(f, links)).ToList());
    }

    public async Task<Result<IEnumerable<FloorResponse>>> GetFloorsForCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<IEnumerable<FloorResponse>>(ErrorCodes.Content.NotFound);

        var links = await BuildLinkContextAsync(campaignId, ct);
        var floors = await floorRepo.GetByCampaignAsync(campaignId, ct);
        return Result.Success<IEnumerable<FloorResponse>>(floors.Select(f => MapFloor(f, links)).ToList());
    }

    public async Task<Result<FloorResponse>> GetFloorByIdAsync(
        Guid gameMasterId, Guid campaignId, Guid floorId, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<FloorResponse>(ErrorCodes.Content.NotFound);

        var floor = await floorRepo.GetByIdAsync(floorId, ct);
        if (floor is null || floor.CampaignId != campaignId)
            return Result.Failure<FloorResponse>(ErrorCodes.Content.NotFound);

        var links = await BuildLinkContextAsync(campaignId, ct);
        return Result.Success(MapFloor(floor, links));
    }

    // ── Floor writes ─────────────────────────────────────────────────────────────

    public async Task<Result<FloorResponse>> CreateFloorAsync(
        Guid gameMasterId, Guid campaignId, CreateFloorRequest request, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<FloorResponse>(ErrorCodes.Content.NotFound);

        var data = request.Data ?? new FloorData();
        var (error, links) = await ValidateAndSanitizeFloorAsync(request.ArcId, data, campaignId, ct);
        if (error is not null)
            return Result.Failure<FloorResponse>(error);

        var floor = new Floor
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            ArcId = request.ArcId,
            Number = request.Number,
            Name = TruncName(request.Name),
            DataJson = JsonSerializer.Serialize(data, JsonOpts),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await floorRepo.AddAsync(floor, ct);
        await floorRepo.SaveChangesAsync(ct);

        return Result.Success(MapFloor(floor, links!));
    }

    public async Task<Result<FloorResponse>> UpdateFloorAsync(
        Guid gameMasterId, Guid campaignId, Guid floorId, UpdateFloorRequest request, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<FloorResponse>(ErrorCodes.Content.NotFound);

        var floor = await floorRepo.GetByIdAsync(floorId, ct);
        if (floor is null || floor.CampaignId != campaignId)
            return Result.Failure<FloorResponse>(ErrorCodes.Content.NotFound);

        var data = request.Data ?? new FloorData();
        var (error, links) = await ValidateAndSanitizeFloorAsync(request.ArcId, data, campaignId, ct);
        if (error is not null)
            return Result.Failure<FloorResponse>(error);

        floor.ArcId = request.ArcId;
        floor.Number = request.Number;
        floor.Name = TruncName(request.Name);
        floor.DataJson = JsonSerializer.Serialize(data, JsonOpts);
        floor.UpdatedAt = DateTime.UtcNow;

        floorRepo.Update(floor);
        await floorRepo.SaveChangesAsync(ct);

        return Result.Success(MapFloor(floor, links!));
    }

    public async Task<Result> DeleteFloorAsync(
        Guid gameMasterId, Guid campaignId, Guid floorId, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure(ErrorCodes.Content.NotFound);

        var floor = await floorRepo.GetByIdAsync(floorId, ct);
        if (floor is null || floor.CampaignId != campaignId)
            return Result.Failure(ErrorCodes.Content.NotFound);

        floorRepo.Remove(floor);
        await floorRepo.SaveChangesAsync(ct);

        // Belt-and-suspenders (reads already degrade a missing floor to null in BuildAsync): if this
        // was the campaign's current-floor pointer, null it so the dashboard drops the dangling
        // reference rather than keeping a pointer to a deleted floor.
        if (campaign.CurrentFloorId == floorId)
        {
            campaign.CurrentFloorId = null;
            campaignRepo.Update(campaign);
            await campaignRepo.SaveChangesAsync(ct);
        }

        return Result.Success();
    }

    // ── Auth ─────────────────────────────────────────────────────────────────────

    private async Task<Campaign?> LoadOwnedCampaignAsync(Guid gameMasterId, Guid campaignId, CancellationToken ct)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        return campaign is not null && campaign.GameMasterId == gameMasterId ? campaign : null;
    }

    // ── Floor validation & sanitizing ─────────────────────────────────────────────

    // Mutates `data` in place (null list elements dropped) and returns the first error code (or null)
    // together with the campaign's link context. The ArcId must belong to this campaign, the
    // ObjectiveType must be a known key, and every linked encounter/reward must be an entity of this
    // campaign. The link dicts are loaded once and reused by the mapper to resolve names.
    private async Task<(string? Error, LinkContext? Links)> ValidateAndSanitizeFloorAsync(
        Guid arcId, FloorData data, Guid campaignId, CancellationToken ct)
    {
        Sanitize(data);

        if (!ContentReference.ObjectiveTypes.Contains(data.ObjectiveType))
            return (ErrorCodes.Content.ObjectiveTypeInvalid, null);

        var arc = await arcRepo.GetByIdAsync(arcId, ct);
        if (arc is null || arc.CampaignId != campaignId)
            return (ErrorCodes.Content.ArcInvalid, null);

        var links = await BuildLinkContextAsync(campaignId, ct);

        if (data.LinkedEncounterIds.Any(id => !links.Encounters.ContainsKey(id)))
            return (ErrorCodes.Content.LinkInvalid, null);
        if (data.LinkedRewardIds.Any(id => !links.Rewards.ContainsKey(id)))
            return (ErrorCodes.Content.LinkInvalid, null);

        return (null, links);
    }

    // Defense-in-depth: drop null secondary-objective lines. LinkedEncounterIds/LinkedRewardIds are
    // Guid lists (value type — no nulls), but null-guarded here to keep the pattern consistent.
    private static void Sanitize(FloorData data)
    {
        data.SecondaryObjectives = (data.SecondaryObjectives ?? []).Where(s => s is not null).ToList();
        data.LinkedEncounterIds = (data.LinkedEncounterIds ?? []).ToList();
        data.LinkedRewardIds = (data.LinkedRewardIds ?? []).ToList();
    }

    private static string TruncName(string? name)
    {
        var value = (name ?? string.Empty).Trim();
        return value.Length > NameMax ? value[..NameMax] : value;
    }

    // ── Link context ──────────────────────────────────────────────────────────────

    // Id→Name dicts for the campaign's encounters and rewards, loaded once and reused for both
    // link validation and read-time name resolution.
    private sealed record LinkContext(
        IReadOnlyDictionary<Guid, string> Encounters,
        IReadOnlyDictionary<Guid, string> Rewards);

    private async Task<LinkContext> BuildLinkContextAsync(Guid campaignId, CancellationToken ct)
    {
        var encounters = await encounterRepo.GetByCampaignAsync(campaignId, ct);
        var rewards = await rewardRepo.GetByCampaignAsync(campaignId, ct);

        var encDict = encounters
            .GroupBy(e => e.Id)
            .ToDictionary(g => g.Key, g => g.First().Name);
        var rewardDict = rewards
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => g.First().Name);

        return new LinkContext(encDict, rewardDict);
    }

    // ── Mapping ────────────────────────────────────────────────────────────────────

    private static ArcResponse MapArc(Arc arc) => new()
    {
        Id = arc.Id,
        Name = arc.Name,
        Order = arc.Order,
        Data = DeserializeArc(arc.DataJson)
    };

    private static FloorResponse MapFloor(Floor floor, LinkContext links)
    {
        var data = FloorDataSerializer.DeserializeFloor(floor.DataJson);

        // Resolve linked ids to names, preserving stored order. Any id that has vanished since it was
        // stored is dropped (writes validated it existed; reads never throw on a stale link).
        var encounters = data.LinkedEncounterIds
            .Where(id => links.Encounters.ContainsKey(id))
            .Select(id => new LinkRef { Id = id, Name = links.Encounters[id] })
            .ToList();
        var rewards = data.LinkedRewardIds
            .Where(id => links.Rewards.ContainsKey(id))
            .Select(id => new LinkRef { Id = id, Name = links.Rewards[id] })
            .ToList();

        return new FloorResponse
        {
            Id = floor.Id,
            ArcId = floor.ArcId,
            Number = floor.Number,
            Name = floor.Name,
            Data = data,
            Encounters = encounters,
            Rewards = rewards
        };
    }

    private static ArcData DeserializeArc(string json)
    {
        ArcData? data;
        try { data = JsonSerializer.Deserialize<ArcData>(json, JsonOpts); }
        catch (JsonException) { data = null; }
        return data ?? new ArcData();
    }
}
