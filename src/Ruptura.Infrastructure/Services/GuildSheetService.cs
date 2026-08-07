using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Guilds;

namespace Ruptura.Infrastructure.Services;

public class GuildSheetService(
    IGuildSheetRepository guildRepo,
    IGuildBuildingRepository buildingRepo,
    IGuildStaffRepository staffRepo,
    ICampaignRepository campaignRepo,
    ICampaignMembershipRepository membershipRepo,
    ICatalogEntryRepository catalogRepo,
    IGuildStatsCalculator calculator) : IGuildSheetService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task<Result<GuildSheetResponse>> GetByCampaignAsync(
        Guid callerId, Guid campaignId, CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null)
            return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.NotFound);

        var isGm = campaign.GameMasterId == callerId;
        var isMember = isGm || await membershipRepo.ExistsAsync(campaignId, callerId, ct);
        if (!isMember)
            return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.NotFound); // hide existence, like CharacterSheet

        var guildResult = await GetOrCreateAsync(campaign, ct);
        if (guildResult.IsFailure)
            return Result.Failure<GuildSheetResponse>(guildResult.Error!);

        return Result.Success(await MapToResponseAsync(guildResult.Value!, ct));
    }

    public async Task<Result<GuildSheetResponse>> UpdateAsync(
        Guid callerId, Guid campaignId, UpdateGuildSheetRequest request, CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null)
            return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.NotFound);

        var isGm = campaign.GameMasterId == callerId;
        var isMember = isGm || await membershipRepo.ExistsAsync(campaignId, callerId, ct);
        if (!isMember)
            return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.NotFound); // hide existence

        var guild = await guildRepo.GetByCampaignAsync(campaignId, ct);
        if (guild is null)
            return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.NotFound);

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
