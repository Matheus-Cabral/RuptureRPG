using System.Text.Json;
using Microsoft.EntityFrameworkCore;
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

        var guild = await GetOrCreateAsync(campaign, ct);
        return Result.Success(await MapToResponseAsync(guild, ct));
    }

    private async Task<GuildSheet> GetOrCreateAsync(Campaign campaign, CancellationToken ct)
    {
        var existing = await guildRepo.GetByCampaignAsync(campaign.Id, ct);
        if (existing is not null) return existing;

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
            return guild;
        }
        catch (DbUpdateException)
        {
            // Concurrent first-access lost the race on ux_guild_sheets_campaign — the winner's row exists.
            return (await guildRepo.GetByCampaignAsync(campaign.Id, ct))!;
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
        data.Legado ??= [];
        return data;
    }
}
