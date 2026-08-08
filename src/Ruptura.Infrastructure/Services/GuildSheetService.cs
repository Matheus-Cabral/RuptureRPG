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
    IResearchProjectRepository researchRepo,
    ICraftingOrderRepository craftingRepo,
    IGuildStatsCalculator calculator,
    IInterludeCalculator interludeCalculator) : IGuildSheetService
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

        // Enforce the GDD reputation range server-side (mirrors the research Points / staff salary
        // clamps) — an out-of-range value on the wire is clamped, never trusted.
        foreach (var rel in incoming.Influence)
            rel.Reputation = Math.Clamp(rel.Reputation, -100, 100);

        var doctrineError = await ValidateDoctrinesAsync(incoming, campaignId, ct);
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
    // Doctrine catalog entry visible to this campaign. The count-vs-limit rule is ADVISORY (mirrors
    // the CS active-building overflow): excess doctrines give no benefit but never block the save —
    // GuildStatsCalculator surfaces DerivedStats.ActiveDoctrineOverflow instead. Returns an error
    // code or null when valid.
    //
    // NOTE: unlike add-installation, an ARCHIVED doctrine stays saveable by design — an id already
    // selected on the sheet must remain persistable even after its catalog entry is archived (the UI
    // keeps it visible). Do not "fix" this to reject IsArchived to match ValidateInstallationAsync.
    private async Task<string?> ValidateDoctrinesAsync(
        GuildSheetData data, Guid campaignId, CancellationToken ct)
    {
        var ids = new List<Guid>(data.ActiveDoctrineIds ?? []);
        if (data.Identity.MainDoctrineId is { } main) ids.Add(main);

        // Every referenced id must be a Doctrine visible to this campaign.
        var distinct = ids.Distinct().ToList();
        if (distinct.Count == 0) return null;

        var entries = (await catalogRepo.GetByIdsAsync(distinct, ct)).ToDictionary(e => e.Id);
        foreach (var id in distinct)
        {
            if (!entries.TryGetValue(id, out var e) || e.Type != CatalogEntryType.Doctrine
                || (e.CampaignId is not null && e.CampaignId != campaignId))
                return ErrorCodes.Guild.DoctrineInvalid;
        }

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

    // Returns the InstallationCatalogData if the entry is a valid, visible, constructible Installation
    // for this campaign; otherwise a failure with the right code.
    // Catalog blobs use DEFAULT (PascalCase) JSON — not the Web convention used for the guild blob.
    // allowArchived: false on ADD (can't build a new archived installation); true on UPDATE (an
    // existing building whose installation was later archived must stay editable/deactivatable —
    // archiving the catalog entry must never permanently freeze a built installation).
    private async Task<Result<InstallationCatalogData>> ValidateInstallationAsync(
        Guid catalogEntryId, Guid campaignId, int level, bool allowArchived, CancellationToken ct)
    {
        var entry = await catalogRepo.GetByIdAsync(catalogEntryId, ct);
        if (entry is null || entry.Type != CatalogEntryType.Installation
            || (!allowArchived && entry.IsArchived)
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

        var validation = await ValidateInstallationAsync(request.CatalogEntryId, campaignId, request.Level, allowArchived: false, ct);
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
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
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

        var validation = await ValidateInstallationAsync(building.CatalogEntryId, campaignId, request.Level, allowArchived: true, ct);
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
        // Enum.IsDefined rejects numeric/undefined values ("5") that TryParse would otherwise accept.
        if (!Enum.TryParse<GuildStaffKind>(request.Kind, out var kind) || !Enum.IsDefined(kind))
            return Result.Failure<GuildStaffResponse>(ErrorCodes.Guild.StaffKindInvalid);

        var staff = new GuildStaff
        {
            Id = Guid.NewGuid(),
            GuildSheetId = guild.Id,
            Kind = kind,
            // Record-keeping posture: TypeOrRanking is persisted verbatim (UI picker supplies canonical values).
            TypeOrRanking = request.TypeOrRanking,
            Name = request.Name,
            // Clamp negatives: a negative salary would perversely reduce daily maintenance.
            DailySalary = Math.Max(0, request.DailySalary),
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

        if (!Enum.TryParse<GuildStaffKind>(request.Kind, out var kind) || !Enum.IsDefined(kind))
            return Result.Failure<GuildStaffResponse>(ErrorCodes.Guild.StaffKindInvalid);

        var staff = await staffRepo.GetByIdAsync(staffId, ct);
        // Cross-guild safety: the target must belong to this campaign's guild, else hide its existence.
        if (staff is null || staff.GuildSheetId != guild.Id)
            return Result.Failure<GuildStaffResponse>(ErrorCodes.Guild.StaffNotFound);

        staff.Kind = kind;
        staff.TypeOrRanking = request.TypeOrRanking;
        staff.Name = request.Name;
        // Clamp negatives: a negative salary would perversely reduce daily maintenance.
        staff.DailySalary = Math.Max(0, request.DailySalary);
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

    public async Task<Result<ResearchProjectResponse>> AddResearchAsync(
        Guid callerId, Guid campaignId, CreateResearchProjectRequest request, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure<ResearchProjectResponse>(auth.Error!);
        var guild = auth.Value!;

        // Complexity/Stage arrive as strings (Shared must not reference the Domain enum). Reject unknown
        // values; Enum.IsDefined guards numeric/undefined strings ("5") that TryParse would accept.
        if (!Enum.TryParse<ResearchComplexity>(request.Complexity, out var complexity) || !Enum.IsDefined(complexity))
            return Result.Failure<ResearchProjectResponse>(ErrorCodes.Guild.ResearchComplexityInvalid);
        if (!Enum.TryParse<ResearchStage>(request.Stage, out var stage) || !Enum.IsDefined(stage))
            return Result.Failure<ResearchProjectResponse>(ErrorCodes.Guild.ResearchStageInvalid);

        var research = new ResearchProject
        {
            Id = Guid.NewGuid(),
            GuildSheetId = guild.Id,
            Name = request.Name,
            // ResearchType is free-form record-keeping (UI picker supplies canonical values) — not validated.
            ResearchType = request.ResearchType,
            Complexity = complexity,
            Stage = stage,
            // RequiredDays is server-authoritative: derived from the complexity tier, never trusted from the wire.
            RequiredDays = ResearchReference.RequiredDays[complexity.ToString()],
            ProgressDays = Math.Max(0, request.ProgressDays),
            Researchers = Math.Max(1, request.Researchers),
            // Top-clamp Points at 1000: keeps CG Pesquisa meaningful and prevents int.MaxValue rows
            // from overflowing the CHECKED Sum in MapToResponseAsync (which would 500 the guild read).
            Points = Math.Clamp(request.Points, 0, 1000),
            IsComplete = request.IsComplete
        };

        await researchRepo.AddAsync(research, ct);
        await researchRepo.SaveChangesAsync(ct);

        return Result.Success(MapResearch(research));
    }

    public async Task<Result<ResearchProjectResponse>> UpdateResearchAsync(
        Guid callerId, Guid campaignId, Guid researchId, UpdateResearchProjectRequest request, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure<ResearchProjectResponse>(auth.Error!);
        var guild = auth.Value!;

        if (!Enum.TryParse<ResearchComplexity>(request.Complexity, out var complexity) || !Enum.IsDefined(complexity))
            return Result.Failure<ResearchProjectResponse>(ErrorCodes.Guild.ResearchComplexityInvalid);
        if (!Enum.TryParse<ResearchStage>(request.Stage, out var stage) || !Enum.IsDefined(stage))
            return Result.Failure<ResearchProjectResponse>(ErrorCodes.Guild.ResearchStageInvalid);

        var research = await researchRepo.GetByIdAsync(researchId, ct);
        // Cross-guild safety: the target must belong to this campaign's guild, else hide its existence.
        if (research is null || research.GuildSheetId != guild.Id)
            return Result.Failure<ResearchProjectResponse>(ErrorCodes.Guild.ResearchNotFound);

        research.Name = request.Name;
        research.ResearchType = request.ResearchType;
        research.Complexity = complexity;
        research.Stage = stage;
        // Re-derive RequiredDays whenever complexity changes — it is never taken from the request.
        research.RequiredDays = ResearchReference.RequiredDays[complexity.ToString()];
        research.ProgressDays = Math.Max(0, request.ProgressDays);
        research.Researchers = Math.Max(1, request.Researchers);
        // Top-clamp Points at 1000 (mirrors AddResearchAsync) — bounds CG Pesquisa and averts overflow.
        research.Points = Math.Clamp(request.Points, 0, 1000);
        research.IsComplete = request.IsComplete;

        researchRepo.Update(research);
        await researchRepo.SaveChangesAsync(ct);

        return Result.Success(MapResearch(research));
    }

    public async Task<Result> DeleteResearchAsync(
        Guid callerId, Guid campaignId, Guid researchId, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure(auth.Error!);
        var guild = auth.Value!;

        var research = await researchRepo.GetByIdAsync(researchId, ct);
        // Cross-guild safety: the target must belong to this campaign's guild, else hide its existence.
        if (research is null || research.GuildSheetId != guild.Id)
            return Result.Failure(ErrorCodes.Guild.ResearchNotFound);

        researchRepo.Remove(research);
        await researchRepo.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static ResearchProjectResponse MapResearch(ResearchProject r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        ResearchType = r.ResearchType,
        Complexity = r.Complexity.ToString(),
        Stage = r.Stage.ToString(),
        ProgressDays = r.ProgressDays,
        RequiredDays = r.RequiredDays,
        Researchers = r.Researchers,
        Points = r.Points,
        IsComplete = r.IsComplete
    };

    public async Task<Result<CraftingOrderResponse>> AddCraftingAsync(
        Guid callerId, Guid campaignId, CreateCraftingOrderRequest request, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure<CraftingOrderResponse>(auth.Error!);
        var guild = auth.Value!;

        // Category/Status arrive as strings (Shared must not reference the Domain enum). Reject unknown
        // values; Enum.IsDefined guards numeric/undefined strings ("5") that TryParse would accept.
        if (!Enum.TryParse<CraftingCategory>(request.Category, out var category) || !Enum.IsDefined(category))
            return Result.Failure<CraftingOrderResponse>(ErrorCodes.Guild.CraftingCategoryInvalid);
        if (!Enum.TryParse<CraftingStatus>(request.Status, out var status) || !Enum.IsDefined(status))
            return Result.Failure<CraftingOrderResponse>(ErrorCodes.Guild.CraftingStatusInvalid);

        var crafting = new CraftingOrder
        {
            Id = Guid.NewGuid(),
            GuildSheetId = guild.Id,
            Category = category,
            ItemName = request.ItemName,
            // Quality is free-form record-keeping (UI picker supplies canonical values) — not validated.
            Quality = request.Quality,
            ProgressDays = Math.Max(0, request.ProgressDays),
            // RequiredDays is client-supplied (no GDD formula) — clamp negatives.
            RequiredDays = Math.Max(0, request.RequiredDays),
            Status = status
        };

        await craftingRepo.AddAsync(crafting, ct);
        await craftingRepo.SaveChangesAsync(ct);

        return Result.Success(MapCrafting(crafting));
    }

    public async Task<Result<CraftingOrderResponse>> UpdateCraftingAsync(
        Guid callerId, Guid campaignId, Guid craftingId, UpdateCraftingOrderRequest request, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure<CraftingOrderResponse>(auth.Error!);
        var guild = auth.Value!;

        if (!Enum.TryParse<CraftingCategory>(request.Category, out var category) || !Enum.IsDefined(category))
            return Result.Failure<CraftingOrderResponse>(ErrorCodes.Guild.CraftingCategoryInvalid);
        if (!Enum.TryParse<CraftingStatus>(request.Status, out var status) || !Enum.IsDefined(status))
            return Result.Failure<CraftingOrderResponse>(ErrorCodes.Guild.CraftingStatusInvalid);

        var crafting = await craftingRepo.GetByIdAsync(craftingId, ct);
        // Cross-guild safety: the target must belong to this campaign's guild, else hide its existence.
        if (crafting is null || crafting.GuildSheetId != guild.Id)
            return Result.Failure<CraftingOrderResponse>(ErrorCodes.Guild.CraftingNotFound);

        crafting.Category = category;
        crafting.ItemName = request.ItemName;
        crafting.Quality = request.Quality;
        crafting.ProgressDays = Math.Max(0, request.ProgressDays);
        crafting.RequiredDays = Math.Max(0, request.RequiredDays);
        crafting.Status = status;

        craftingRepo.Update(crafting);
        await craftingRepo.SaveChangesAsync(ct);

        return Result.Success(MapCrafting(crafting));
    }

    public async Task<Result> DeleteCraftingAsync(
        Guid callerId, Guid campaignId, Guid craftingId, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure(auth.Error!);
        var guild = auth.Value!;

        var crafting = await craftingRepo.GetByIdAsync(craftingId, ct);
        // Cross-guild safety: the target must belong to this campaign's guild, else hide its existence.
        if (crafting is null || crafting.GuildSheetId != guild.Id)
            return Result.Failure(ErrorCodes.Guild.CraftingNotFound);

        craftingRepo.Remove(crafting);
        await craftingRepo.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static CraftingOrderResponse MapCrafting(CraftingOrder c) => new()
    {
        Id = c.Id,
        Category = c.Category.ToString(),
        ItemName = c.ItemName,
        Quality = c.Quality,
        ProgressDays = c.ProgressDays,
        RequiredDays = c.RequiredDays,
        Status = c.Status.ToString()
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
    // Enum.IsDefined guards against numeric/undefined strings ("5") that TryParse would accept —
    // a truly-unparseable Kind falls back to Principal.
    private static ExpeditionKind ParseKind(string kind) =>
        Enum.TryParse<ExpeditionKind>(kind, out var k) && Enum.IsDefined(k) ? k : ExpeditionKind.Principal;

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

        // Only COMPLETED research projects contribute their Points to the CG Pesquisa term.
        var research = (await researchRepo.GetByGuildAsync(guild.Id, ct)).ToList();
        var researchPoints = research.Where(r => r.IsComplete).Sum(r => r.Points);
        var derived = calculator.Calculate(data, buildings, staff, researchPoints, installationCatalog);

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
            Research = research.Select(MapResearch).ToList(),
            Crafting = (await craftingRepo.GetByGuildAsync(guild.Id, ct)).Select(MapCrafting).ToList(),
            Version = guild.Version,
            CreatedAt = guild.CreatedAt,
            UpdatedAt = guild.UpdatedAt
        };
    }

    private const int MaxInterludeDays = 3650;

    // Builds the interlude projection from FRESH state (buildings/staff/research/crafting reloaded,
    // derived stats recomputed). Shared by preview and apply so the apply always recomputes the
    // numbers server-side rather than trusting anything on the wire.
    private async Task<Result<(GuildSheet Guild, InterludeProjection Projection, GuildSheetData Data)>>
        BuildInterludeProjectionAsync(Guid callerId, Guid campaignId, int days, CancellationToken ct)
    {
        if (days < 1 || days > MaxInterludeDays)
            return Result.Failure<(GuildSheet, InterludeProjection, GuildSheetData)>(ErrorCodes.Guild.InterludeDaysInvalid);

        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure)
            return Result.Failure<(GuildSheet, InterludeProjection, GuildSheetData)>(auth.Error!);
        var guild = auth.Value!;

        var data = Deserialize(guild.DataJson);
        var buildings = (await buildingRepo.GetByGuildAsync(guild.Id, ct)).ToList();
        var staff = (await staffRepo.GetByGuildAsync(guild.Id, ct)).ToList();
        var research = (await researchRepo.GetByGuildAsync(guild.Id, ct)).ToList();
        var crafting = (await craftingRepo.GetByGuildAsync(guild.Id, ct)).ToList();

        var installationIds = buildings.Select(b => b.CatalogEntryId).Distinct().ToList();
        var installationCatalog = installationIds.Count == 0
            ? new Dictionary<Guid, CatalogEntry>()
            : (await catalogRepo.GetByIdsAsync(installationIds, ct)).ToDictionary(e => e.Id);
        var researchPoints = research.Where(r => r.IsComplete).Sum(r => r.Points);
        var derived = calculator.Calculate(data, buildings, staff, researchPoints, installationCatalog);

        var projection = interludeCalculator.Project(derived, research, crafting, days);
        return Result.Success((guild, projection, data));
    }

    public async Task<Result<InterludeProjection>> PreviewInterludeAsync(
        Guid callerId, Guid campaignId, int days, CancellationToken ct = default)
    {
        var built = await BuildInterludeProjectionAsync(callerId, campaignId, days, ct);
        return built.IsFailure
            ? Result.Failure<InterludeProjection>(built.Error!)
            : Result.Success(built.Value.Projection);
    }

    public async Task<Result<GuildSheetResponse>> ApplyInterludeAsync(
        Guid callerId, Guid campaignId, ApplyInterludeRequest request, CancellationToken ct = default)
    {
        var validKinds = new[] { "Maintenance", "Income", "ResearchProgress", "CraftingProgress" };
        if (!validKinds.Contains(request.Kind))
            return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.InterludeKindInvalid);

        var built = await BuildInterludeProjectionAsync(callerId, campaignId, request.Days, ct);
        if (built.IsFailure)
            return Result.Failure<GuildSheetResponse>(built.Error!);
        var (guild, projection, data) = built.Value;

        // Select the SERVER-computed indicator matching the client's selector. No client number is trusted.
        var indicator = projection.Indicators.FirstOrDefault(i =>
            i.Kind == request.Kind && i.TargetId == request.TargetId);
        if (indicator is null)
            return Result.Failure<GuildSheetResponse>(request.Kind switch
            {
                "ResearchProgress" => ErrorCodes.Guild.ResearchNotFound,
                "CraftingProgress" => ErrorCodes.Guild.CraftingNotFound,
                _ => ErrorCodes.Guild.InterludeKindInvalid
            });

        switch (request.Kind)
        {
            case "Maintenance":
            case "Income":
                // Widen to long before adding so a near-int.MaxValue balance + positive income can't
                // wrap negative (which Math.Max(0,…) would then zero out); clamp back into int range.
                data.Resources.Silver = (int)Math.Clamp((long)data.Resources.Silver + (indicator.SilverDelta ?? 0), 0, int.MaxValue);
                guild.DataJson = JsonSerializer.Serialize(data, JsonOpts);
                guild.UpdatedAt = DateTime.UtcNow;
                // Check the CLIENT's expected version (not guild.Version, which would compare the row
                // against itself — a no-op). A concurrent blob save since the client's load → 409.
                guildRepo.SetExpectedVersion(guild, request.Version);
                guildRepo.Update(guild);
                try { await guildRepo.SaveChangesAsync(ct); }
                catch (DbUpdateConcurrencyException) { return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.Conflict); }
                break;

            case "ResearchProgress":
            {
                var r = (await researchRepo.GetByGuildAsync(guild.Id, ct)).FirstOrDefault(x => x.Id == request.TargetId);
                if (r is null || r.IsComplete)
                    return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.ResearchNotFound);
                r.ProgressDays += indicator.DaysAdded ?? 0;
                if (r.ProgressDays >= r.RequiredDays) { r.ProgressDays = r.RequiredDays; r.IsComplete = true; }
                researchRepo.Update(r);
                await researchRepo.SaveChangesAsync(ct);
                break;
            }

            case "CraftingProgress":
            {
                var c = (await craftingRepo.GetByGuildAsync(guild.Id, ct)).FirstOrDefault(x => x.Id == request.TargetId);
                if (c is null || c.Status != CraftingStatus.EmAndamento)
                    return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.CraftingNotFound);
                c.ProgressDays += indicator.DaysAdded ?? 0;
                if (c.ProgressDays >= c.RequiredDays) { c.ProgressDays = c.RequiredDays; c.Status = CraftingStatus.Concluido; }
                craftingRepo.Update(c);
                await craftingRepo.SaveChangesAsync(ct);
                break;
            }
        }

        // Re-map from fresh state (need a fresh guild fetch for the maintenance/income Version bump).
        var refreshed = await guildRepo.GetByCampaignAsync(campaignId, ct);
        return Result.Success(await MapToResponseAsync(refreshed!, ct));
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
