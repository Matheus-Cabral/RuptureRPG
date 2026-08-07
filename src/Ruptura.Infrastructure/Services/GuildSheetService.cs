using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Guilds;

namespace Ruptura.Infrastructure.Services;

public class GuildSheetService(
    IGuildSheetRepository guildRepo,
    IGuildBuildingRepository buildingRepo,
    IGuildStaffRepository staffRepo,
    ICampaignRepository campaignRepo,
    ICampaignMembershipRepository membershipRepo,
    ICatalogEntryRepository catalogRepo,
    IExpeditionRepository expeditionRepo,
    IGuildStatsCalculator calculator) : IGuildSheetService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task<Result<GuildSheetResponse>> GetByCampaignAsync(
        Guid callerId, Guid campaignId, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure<GuildSheetResponse>(auth.Error!);

        return Result.Success(await MapToResponseAsync(auth.Value!, ct));
    }

    public async Task<Result<GuildSheetResponse>> UpdateAsync(
        Guid callerId, Guid campaignId, UpdateGuildSheetRequest request, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure<GuildSheetResponse>(auth.Error!);
        var guild = auth.Value!;

        // EmblemImagePath is server-authoritative — preserve the stored value, ignore the
        // client's (emblem changes only via POST /api/media). Mirrors PortraitImagePath.
        var stored = Deserialize(guild.DataJson);
        var incoming = Deserialize(request.DataJson);
        incoming.Identity.EmblemImagePath = stored.Identity.EmblemImagePath;

        guild.GuildName = request.GuildName;
        guild.DataJson = JsonSerializer.Serialize(incoming, JsonOpts);
        guild.UpdatedAt = DateTime.UtcNow;

        guildRepo.SetExpectedVersion(guild, request.Version);
        guildRepo.Update(guild);
        try
        {
            await guildRepo.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // A concurrent write advanced xmin → UPDATE matched 0 rows. The caller's payload is
            // built on a stale version; reject rather than silently clobber the winner's changes.
            return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.Conflict);
        }

        return Result.Success(await MapToResponseAsync(guild, ct));
    }

    public async Task<Result<ExpeditionResponse>> AddExpeditionAsync(
        Guid callerId, Guid campaignId, CreateExpeditionRequest request, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure<ExpeditionResponse>(auth.Error!);
        var guild = auth.Value!;

        var expedition = new Expedition
        {
            Id = Guid.NewGuid(),
            GuildSheetId = guild.Id,
            Kind = ParseKind(request.Kind),
            Date = Utc(request.Date),
            Participants = request.Participants,
            Objective = request.Objective,
            Result = request.Result,
            Losses = request.Losses,
            ResourcesGained = request.ResourcesGained
        };

        await expeditionRepo.AddAsync(expedition, ct);
        await expeditionRepo.SaveChangesAsync(ct);

        return Result.Success(MapExpedition(expedition));
    }

    public async Task<Result<ExpeditionResponse>> UpdateExpeditionAsync(
        Guid callerId, Guid campaignId, Guid expeditionId, UpdateExpeditionRequest request, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure<ExpeditionResponse>(auth.Error!);
        var guild = auth.Value!;

        var expedition = await expeditionRepo.GetByIdAsync(expeditionId, ct);
        // Cross-guild safety: the target must belong to this campaign's guild, else hide its existence.
        if (expedition is null || expedition.GuildSheetId != guild.Id)
            return Result.Failure<ExpeditionResponse>(ErrorCodes.Guild.NotFound);

        expedition.Kind = ParseKind(request.Kind);
        expedition.Date = Utc(request.Date);
        expedition.Participants = request.Participants;
        expedition.Objective = request.Objective;
        expedition.Result = request.Result;
        expedition.Losses = request.Losses;
        expedition.ResourcesGained = request.ResourcesGained;

        expeditionRepo.Update(expedition);
        await expeditionRepo.SaveChangesAsync(ct);

        return Result.Success(MapExpedition(expedition));
    }

    public async Task<Result> DeleteExpeditionAsync(
        Guid callerId, Guid campaignId, Guid expeditionId, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure(auth.Error!);
        var guild = auth.Value!;

        var expedition = await expeditionRepo.GetByIdAsync(expeditionId, ct);
        // Cross-guild safety: the target must belong to this campaign's guild, else hide its existence.
        if (expedition is null || expedition.GuildSheetId != guild.Id)
            return Result.Failure(ErrorCodes.Guild.NotFound);

        expeditionRepo.Remove(expedition);
        await expeditionRepo.SaveChangesAsync(ct);

        return Result.Success();
    }

    // Shared-write auth (GM or campaign member) + guild resolution, reused by every guild mutation.
    private async Task<Result<GuildSheet>> AuthorizeAsync(Guid callerId, Guid campaignId, CancellationToken ct)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null)
            return Result.Failure<GuildSheet>(ErrorCodes.Guild.NotFound);

        var isGm = campaign.GameMasterId == callerId;
        var isMember = isGm || await membershipRepo.ExistsAsync(campaignId, callerId, ct);
        if (!isMember)
            return Result.Failure<GuildSheet>(ErrorCodes.Guild.NotFound); // hide existence, like CharacterSheet

        return await GetOrCreateAsync(campaign, ct);
    }

    // Npgsql rejects a non-UTC Kind on a timestamptz column; normalize before saving.
    private static DateTime Utc(DateTime d) =>
        d.Kind == DateTimeKind.Utc ? d : DateTime.SpecifyKind(d, DateTimeKind.Utc);

    // Shared must not reference the Domain enum, so Kind is a string on the wire; parse leniently.
    private static ExpeditionKind ParseKind(string kind) =>
        Enum.TryParse<ExpeditionKind>(kind, out var k) ? k : ExpeditionKind.Principal;

    private static ExpeditionResponse MapExpedition(Expedition e) => new()
    {
        Id = e.Id,
        Kind = e.Kind.ToString(),
        Date = e.Date,
        Participants = e.Participants,
        Objective = e.Objective,
        Result = e.Result,
        Losses = e.Losses,
        ResourcesGained = e.ResourcesGained
    };

    private async Task<Result<GuildSheet>> GetOrCreateAsync(Campaign campaign, CancellationToken ct)
    {
        var existing = await guildRepo.GetByCampaignAsync(campaign.Id, ct);
        if (existing is not null) return Result.Success(existing);

        var guild = new GuildSheet
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            GuildName = campaign.Name,          // seed from campaign name; editable later (sub-plan #3)
            CreatedByGameMasterId = campaign.GameMasterId,
            DataJson = "{}"
        };
        try
        {
            await guildRepo.AddAsync(guild, ct);
            await guildRepo.SaveChangesAsync(ct);
            return Result.Success(guild);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Concurrent first-access lost the race on ux_guild_sheets_campaign — the winner's row exists.
            // Detach the doomed Added entity so a later SaveChanges in this scope won't re-attempt the INSERT.
            guildRepo.Detach(guild);
            var winner = await guildRepo.GetByCampaignAsync(campaign.Id, ct);
            // The winner's campaign may have been concurrently deleted — signal NotFound rather than deref null.
            return winner is not null
                ? Result.Success(winner)
                : Result.Failure<GuildSheet>(ErrorCodes.Guild.NotFound);
        }
    }

    private async Task<GuildSheetResponse> MapToResponseAsync(GuildSheet guild, CancellationToken ct)
    {
        var data = Deserialize(guild.DataJson);
        var buildings = (await buildingRepo.GetByGuildAsync(guild.Id, ct)).ToList();
        var staff = (await staffRepo.GetByGuildAsync(guild.Id, ct)).ToList();

        var installationIds = buildings.Select(b => b.CatalogEntryId).Distinct().ToList();
        var installationCatalog = installationIds.Count == 0
            ? new Dictionary<Guid, CatalogEntry>()
            : (await catalogRepo.GetByIdsAsync(installationIds, ct)).ToDictionary(e => e.Id);

        // No research projects until sub-plan #5 -> researchPoints = 0.
        var derived = calculator.Calculate(data, buildings, staff, researchPoints: 0, installationCatalog);

        return new GuildSheetResponse
        {
            Id = guild.Id,
            CampaignId = guild.CampaignId,
            GuildName = guild.GuildName,
            Data = data,
            DerivedStats = derived,
            Expeditions = (await expeditionRepo.GetByGuildAsync(guild.Id, ct)).Select(MapExpedition).ToList(),
            Version = guild.Version,
            CreatedAt = guild.CreatedAt,
            UpdatedAt = guild.UpdatedAt
        };
    }

    // Guarantee every blob module is non-null at the boundary (character-sheet #3 lesson).
    private static GuildSheetData Deserialize(string json)
    {
        GuildSheetData? data;
        try { data = JsonSerializer.Deserialize<GuildSheetData>(json, JsonOpts); }
        catch (JsonException) { data = null; }
        data ??= new GuildSheetData();
        data.Identity ??= new GuildIdentity();
        data.Prestige ??= new GuildPrestige();
        data.Influence ??= [];
        data.Resources ??= new GuildResources();
        data.Resources.Materials ??= [];
        data.Resources.Artifacts ??= [];
        data.ActiveDoctrineIds ??= [];
        data.Knowledge ??= new GuildKnowledge();
        data.Knowledge.Maps ??= [];
        data.Knowledge.Recipes ??= [];
        data.Knowledge.CataloguedEnemies ??= [];
        data.Knowledge.DefeatedBosses ??= [];
        data.Knowledge.HistoricalRecords ??= [];
        data.Legado ??= [];
        return data;
    }
}
