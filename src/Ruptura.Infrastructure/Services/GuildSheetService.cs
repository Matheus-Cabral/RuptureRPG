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

        var doctrineError = await ValidateDoctrinesAsync(incoming, guild.Id, campaignId, ct);
        if (doctrineError is not null)
            return Result.Failure<GuildSheetResponse>(doctrineError);

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

    // Every referenced doctrine id (ActiveDoctrineIds + Identity.MainDoctrineId) must resolve to a
    // Doctrine catalog entry visible to this campaign; ActiveDoctrineIds count must be within the
    // derived limit min(4, 2 + Câmara do Conselho level). Returns an error code or null when valid.
    private async Task<string?> ValidateDoctrinesAsync(
        GuildSheetData data, Guid guildSheetId, Guid campaignId, CancellationToken ct)
    {
        var ids = new List<Guid>(data.ActiveDoctrineIds ?? []);
        if (data.Identity.MainDoctrineId is { } main) ids.Add(main);
        if (ids.Count == 0 && (data.ActiveDoctrineIds?.Count ?? 0) == 0) return null;

        // Every referenced id must be a Doctrine visible to this campaign.
        var distinct = ids.Distinct().ToList();
        if (distinct.Count > 0)
        {
            var entries = (await catalogRepo.GetByIdsAsync(distinct, ct)).ToDictionary(e => e.Id);
            foreach (var id in distinct)
            {
                if (!entries.TryGetValue(id, out var e) || e.Type != CatalogEntryType.Doctrine
                    || (e.CampaignId is not null && e.CampaignId != campaignId))
                    return ErrorCodes.Guild.DoctrineInvalid;
            }
        }

        // ActiveDoctrineIds count must be within the derived limit (min(4, 2 + Câmara level)).
        // camaraLevel uses IsActive to match the calculator's LevelOf (benefits exclude inactive
        // buildings — sub-plan #2 rule).
        var buildings = (await buildingRepo.GetByGuildAsync(guildSheetId, ct)).ToList();
        var camaraLevel = buildings
            .Where(b => b.CatalogEntryId == GuildCatalogIds.CamaraDoConselho && b.IsActive)
            .Select(b => b.Level).FirstOrDefault();
        var limit = Math.Min(4, 2 + camaraLevel);
        if ((data.ActiveDoctrineIds?.Count ?? 0) > limit)
            return ErrorCodes.Guild.DoctrineLimitExceeded;

        return null;
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

    // Returns the InstallationCatalogData if the entry is a valid, visible, non-archived,
    // constructible Installation for this campaign; otherwise a failure with the right code.
    // Catalog blobs use DEFAULT (PascalCase) JSON — not the Web convention used for the guild blob.
    private async Task<Result<InstallationCatalogData>> ValidateInstallationAsync(
        Guid catalogEntryId, Guid campaignId, int level, CancellationToken ct)
    {
        var entry = await catalogRepo.GetByIdAsync(catalogEntryId, ct);
        if (entry is null || entry.Type != CatalogEntryType.Installation
            || entry.IsArchived
            || (entry.CampaignId is not null && entry.CampaignId != campaignId))
            return Result.Failure<InstallationCatalogData>(ErrorCodes.Guild.InstallationInvalid);

        InstallationCatalogData? data;
        try { data = JsonSerializer.Deserialize<InstallationCatalogData>(entry.DataJson); }
        catch (JsonException) { data = null; }
        if (data is null)
            return Result.Failure<InstallationCatalogData>(ErrorCodes.Guild.InstallationInvalid);
        if (data.NonConstructible)
            return Result.Failure<InstallationCatalogData>(ErrorCodes.Guild.BuildingNotConstructible);
        if (level < 1 || level > data.LevelCap)
            return Result.Failure<InstallationCatalogData>(ErrorCodes.Guild.BuildingLevelInvalid);
        return Result.Success(data);
    }

    public async Task<Result<GuildBuildingResponse>> AddBuildingAsync(
        Guid callerId, Guid campaignId, CreateBuildingRequest request, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure<GuildBuildingResponse>(auth.Error!);
        var guild = auth.Value!;

        var validation = await ValidateInstallationAsync(request.CatalogEntryId, campaignId, request.Level, ct);
        if (validation.IsFailure)
            return Result.Failure<GuildBuildingResponse>(validation.Error!);

        var building = new GuildBuilding
        {
            Id = Guid.NewGuid(),
            GuildSheetId = guild.Id,
            CatalogEntryId = request.CatalogEntryId,
            Level = request.Level,
            IsActive = request.IsActive
        };
        try
        {
            await buildingRepo.AddAsync(building, ct);
            await buildingRepo.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // ux_guild_buildings_sheet_installation — one building per installation type.
            return Result.Failure<GuildBuildingResponse>(ErrorCodes.Guild.BuildingExists);
        }

        return Result.Success(await MapBuildingAsync(building, ct));
    }

    public async Task<Result<GuildBuildingResponse>> UpdateBuildingAsync(
        Guid callerId, Guid campaignId, Guid buildingId, UpdateBuildingRequest request, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure<GuildBuildingResponse>(auth.Error!);
        var guild = auth.Value!;

        var building = await buildingRepo.GetByIdAsync(buildingId, ct);
        // Cross-guild safety: the target must belong to this campaign's guild, else hide its existence.
        if (building is null || building.GuildSheetId != guild.Id)
            return Result.Failure<GuildBuildingResponse>(ErrorCodes.Guild.BuildingNotFound);

        var validation = await ValidateInstallationAsync(building.CatalogEntryId, campaignId, request.Level, ct);
        if (validation.IsFailure)
            return Result.Failure<GuildBuildingResponse>(validation.Error!);

        building.Level = request.Level;
        building.IsActive = request.IsActive;
        buildingRepo.Update(building);
        await buildingRepo.SaveChangesAsync(ct);

        return Result.Success(await MapBuildingAsync(building, ct));
    }

    public async Task<Result> DeleteBuildingAsync(
        Guid callerId, Guid campaignId, Guid buildingId, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure(auth.Error!);
        var guild = auth.Value!;

        var building = await buildingRepo.GetByIdAsync(buildingId, ct);
        // Cross-guild safety: the target must belong to this campaign's guild, else hide its existence.
        if (building is null || building.GuildSheetId != guild.Id)
            return Result.Failure(ErrorCodes.Guild.BuildingNotFound);

        buildingRepo.Remove(building);
        await buildingRepo.SaveChangesAsync(ct);

        return Result.Success();
    }

    private async Task<GuildBuildingResponse> MapBuildingAsync(GuildBuilding b, CancellationToken ct)
    {
        var entry = await catalogRepo.GetByIdAsync(b.CatalogEntryId, ct);
        return MapBuilding(b, entry?.Name ?? string.Empty);
    }

    private static GuildBuildingResponse MapBuilding(GuildBuilding b, string installationName) => new()
    {
        Id = b.Id,
        CatalogEntryId = b.CatalogEntryId,
        InstallationName = installationName,
        Level = b.Level,
        IsActive = b.IsActive
    };

    public async Task<Result<GuildStaffResponse>> AddStaffAsync(
        Guid callerId, Guid campaignId, CreateStaffRequest request, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure<GuildStaffResponse>(auth.Error!);
        var guild = auth.Value!;

        // Shared must not reference the Domain enum, so Kind is a string on the wire; reject unknown.
        if (!Enum.TryParse<GuildStaffKind>(request.Kind, out var kind))
            return Result.Failure<GuildStaffResponse>(ErrorCodes.Guild.StaffKindInvalid);

        var staff = new GuildStaff
        {
            Id = Guid.NewGuid(),
            GuildSheetId = guild.Id,
            Kind = kind,
            // Record-keeping posture: TypeOrRanking is persisted verbatim (UI picker supplies canonical values).
            TypeOrRanking = request.TypeOrRanking,
            Name = request.Name,
            DailySalary = request.DailySalary,
            IsActive = request.IsActive,
            Efficiency = request.Efficiency,
            Morale = request.Morale
        };

        await staffRepo.AddAsync(staff, ct);
        await staffRepo.SaveChangesAsync(ct);

        return Result.Success(MapStaff(staff));
    }

    public async Task<Result<GuildStaffResponse>> UpdateStaffAsync(
        Guid callerId, Guid campaignId, Guid staffId, UpdateStaffRequest request, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure<GuildStaffResponse>(auth.Error!);
        var guild = auth.Value!;

        if (!Enum.TryParse<GuildStaffKind>(request.Kind, out var kind))
            return Result.Failure<GuildStaffResponse>(ErrorCodes.Guild.StaffKindInvalid);

        var staff = await staffRepo.GetByIdAsync(staffId, ct);
        // Cross-guild safety: the target must belong to this campaign's guild, else hide its existence.
        if (staff is null || staff.GuildSheetId != guild.Id)
            return Result.Failure<GuildStaffResponse>(ErrorCodes.Guild.StaffNotFound);

        staff.Kind = kind;
        staff.TypeOrRanking = request.TypeOrRanking;
        staff.Name = request.Name;
        staff.DailySalary = request.DailySalary;
        staff.IsActive = request.IsActive;
        staff.Efficiency = request.Efficiency;
        staff.Morale = request.Morale;

        staffRepo.Update(staff);
        await staffRepo.SaveChangesAsync(ct);

        return Result.Success(MapStaff(staff));
    }

    public async Task<Result> DeleteStaffAsync(
        Guid callerId, Guid campaignId, Guid staffId, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure(auth.Error!);
        var guild = auth.Value!;

        var staff = await staffRepo.GetByIdAsync(staffId, ct);
        // Cross-guild safety: the target must belong to this campaign's guild, else hide its existence.
        if (staff is null || staff.GuildSheetId != guild.Id)
            return Result.Failure(ErrorCodes.Guild.StaffNotFound);

        staffRepo.Remove(staff);
        await staffRepo.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static GuildStaffResponse MapStaff(GuildStaff s) => new()
    {
        Id = s.Id,
        Kind = s.Kind.ToString(),
        TypeOrRanking = s.TypeOrRanking,
        Name = s.Name,
        DailySalary = s.DailySalary,
        IsActive = s.IsActive,
        Efficiency = s.Efficiency,
        Morale = s.Morale
    };

    public async Task<Result<GuildSheet>> AuthorizeGuildAccessByIdAsync(
        Guid callerId, Guid guildSheetId, CancellationToken ct = default)
    {
        var guild = await guildRepo.GetByIdAsync(guildSheetId, ct);
        if (guild is null)
            return Result.Failure<GuildSheet>(ErrorCodes.Guild.NotFound);

        var campaign = await campaignRepo.GetByIdAsync(guild.CampaignId, ct);
        var isGm = campaign?.GameMasterId == callerId;
        var isMember = isGm || await membershipRepo.ExistsAsync(guild.CampaignId, callerId, ct);
        if (!isMember)
            return Result.Failure<GuildSheet>(ErrorCodes.Guild.NotFound);

        return Result.Success(guild);
    }

    // No auth of its own — MediaController authorizes via AuthorizeGuildAccessByIdAsync first
    // (mirrors CharacterSheetService.SetPortraitPathAsync). Sets Identity.EmblemImagePath inside
    // the blob (there is no dedicated column), preserving all other blob data. Version-checkpointed:
    // a stale expectedVersion → Guild.Conflict rather than a lost update or an unhandled 500.
    public async Task<Result<uint>> SetEmblemPathAsync(
        Guid guildSheetId, string path, uint expectedVersion, CancellationToken ct = default)
    {
        var guild = await guildRepo.GetByIdAsync(guildSheetId, ct);
        if (guild is null)
            return Result.Failure<uint>(ErrorCodes.Guild.NotFound);

        var data = Deserialize(guild.DataJson);
        data.Identity.EmblemImagePath = path;
        guild.DataJson = JsonSerializer.Serialize(data, JsonOpts);
        guild.UpdatedAt = DateTime.UtcNow;

        guildRepo.SetExpectedVersion(guild, expectedVersion);
        guildRepo.Update(guild);
        try
        {
            await guildRepo.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // A concurrent write advanced xmin → UPDATE matched 0 rows. Reject rather than
            // reintroduce the lost-update the write path already guards against.
            return Result.Failure<uint>(ErrorCodes.Guild.Conflict);
        }

        // xmin is refreshed on the tracked entity after SaveChanges (ValueGeneratedOnAddOrUpdate).
        return Result.Success(guild.Version);
    }

    // Reads the current emblem path through the guarded Deserialize so MediaController does not
    // deserialize the blob inline (closes the duplicate-parse minor).
    public async Task<Result<string?>> GetEmblemPathAsync(Guid guildSheetId, CancellationToken ct = default)
    {
        var guild = await guildRepo.GetByIdAsync(guildSheetId, ct);
        if (guild is null)
            return Result.Failure<string?>(ErrorCodes.Guild.NotFound);

        return Result.Success<string?>(Deserialize(guild.DataJson).Identity.EmblemImagePath);
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
    // Local → convert instant-preserving; Unspecified → assume already UTC and just stamp the Kind.
    private static DateTime Utc(DateTime d) => d.Kind switch
    {
        DateTimeKind.Utc => d,
        DateTimeKind.Local => d.ToUniversalTime(),
        _ => DateTime.SpecifyKind(d, DateTimeKind.Utc)
    };

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
            // Reuse the installationCatalog dict already loaded for the calculator — no N+1 GetByIdAsync.
            Buildings = buildings
                .Select(b => MapBuilding(b, installationCatalog.TryGetValue(b.CatalogEntryId, out var e) ? e.Name : string.Empty))
                .ToList(),
            Staff = staff.Select(MapStaff).ToList(),
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
