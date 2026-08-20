using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Catalog;

namespace Ruptura.Infrastructure.Services;

public class CatalogEntryService(
    ICatalogEntryRepository catalogRepo,
    ICampaignRepository campaignRepo,
    ICampaignMembershipRepository membershipRepo) : ICatalogEntryService
{
    public async Task<Result<IEnumerable<CatalogEntryResponse>>> GetByTypeAsync(
        Guid callerId,
        string type,
        Guid campaignId,
        bool includeArchived,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<CatalogEntryType>(type, out var parsedType) || !Enum.IsDefined(parsedType))
            return Result.Failure<IEnumerable<CatalogEntryResponse>>(ErrorCodes.Catalog.InvalidType);

        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null)
            return Result.Failure<IEnumerable<CatalogEntryResponse>>(ErrorCodes.Catalog.NotFound);

        var isGameMaster = campaign.GameMasterId == callerId;
        var isMember = isGameMaster || await membershipRepo.ExistsAsync(campaignId, callerId, ct);
        if (!isMember)
            return Result.Failure<IEnumerable<CatalogEntryResponse>>(ErrorCodes.Catalog.NotFound);

        var entries = await catalogRepo.GetByTypeAsync(parsedType, campaignId, includeArchived, ct);
        // The GM sees everything, including private homebrew drafts, so they can work on them
        // before publishing. Players (and any other non-GM member) only see IsPublic entries —
        // global/official entries are always IsPublic (see CatalogEntry.IsPublic), so this never
        // hides core rulebook content, only a GM's unpublished homebrew.
        if (!isGameMaster) entries = entries.Where(e => e.IsPublic);
        return Result.Success(entries.Select(MapToResponse));
    }

    public async Task<Result<CatalogEntryResponse>> CreateAsync(
        Guid gameMasterId,
        CreateCatalogEntryRequest request,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<CatalogEntryType>(request.Type, out var parsedType) || !Enum.IsDefined(parsedType))
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.InvalidType);

        var campaign = await campaignRepo.GetByIdAsync(request.CampaignId, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.NotFound);

        if (await catalogRepo.ExistsAsync(parsedType, request.CampaignId, request.Name, ct)
            || await catalogRepo.ExistsAsync(parsedType, null, request.Name, ct))
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.AlreadyExists);

        var entry = new CatalogEntry
        {
            Id = Guid.NewGuid(),
            Type = parsedType,
            CampaignId = request.CampaignId,
            Name = request.Name,
            DataJson = request.DataJson,
            IsPublic = request.IsPublic,
            CreatedByGameMasterId = gameMasterId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await catalogRepo.AddAsync(entry, ct);
        await catalogRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(entry));
    }

    public async Task<Result<CatalogEntryResponse>> UpdateAsync(
        Guid gameMasterId,
        Guid entryId,
        UpdateCatalogEntryRequest request,
        CancellationToken ct = default)
    {
        var entry = await catalogRepo.GetByIdAsync(entryId, ct);
        if (entry is null)
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.NotFound);

        // Global entries are normally immutable (they're one shared row used by every campaign
        // in the system, not owned by any single GM). Spell and Technique are the sole exception,
        // by explicit request: any authenticated GM may edit the official spell/technique library
        // directly (e.g. to fill in Damage) — accepting that the edit is visible to every campaign,
        // not just theirs. Every other global type (Origin, Skill, EquipmentItem, ...) stays
        // fully protected. There's no per-campaign ownership check for this path since global
        // entries have none — [Authorize(Roles = "GameMaster")] on the controller is the only gate.
        var isEditableGlobalType = entry.Type is CatalogEntryType.Spell or CatalogEntryType.Technique;
        if (entry.CampaignId is null && !isEditableGlobalType)
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.CannotModifyGlobalEntry);

        if (entry.IsArchived)
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.AlreadyArchived);

        if (entry.CampaignId is { } campaignId)
        {
            var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
            if (campaign is null || campaign.GameMasterId != gameMasterId)
                return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.NotFound);
        }

        if (!string.Equals(entry.Name, request.Name, StringComparison.Ordinal)
            && (await catalogRepo.ExistsAsync(entry.Type, entry.CampaignId, request.Name, ct)
                || await catalogRepo.ExistsAsync(entry.Type, null, request.Name, ct)))
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.AlreadyExists);

        entry.Name = request.Name;
        entry.DataJson = request.DataJson;
        // A global entry stays public no matter what the form sends — it's shared by every
        // campaign, so "private" would have to mean "invisible to every player everywhere",
        // which isn't a state this app has a use for (see CatalogEntry.IsPublic).
        entry.IsPublic = entry.CampaignId is null || request.IsPublic;
        entry.UpdatedAt = DateTime.UtcNow;
        catalogRepo.Update(entry);
        await catalogRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(entry));
    }

    public async Task<Result> DeleteAsync(
        Guid gameMasterId,
        Guid entryId,
        CancellationToken ct = default)
    {
        var entry = await catalogRepo.GetByIdAsync(entryId, ct);
        if (entry is null)
            return Result.Failure(ErrorCodes.Catalog.NotFound);

        if (entry.CampaignId is null)
            return Result.Failure(ErrorCodes.Catalog.CannotModifyGlobalEntry);

        if (entry.IsArchived)
            return Result.Failure(ErrorCodes.Catalog.AlreadyArchived);

        var campaign = await campaignRepo.GetByIdAsync(entry.CampaignId.Value, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure(ErrorCodes.Catalog.NotFound);

        entry.IsArchived = true;
        entry.UpdatedAt = DateTime.UtcNow;
        catalogRepo.Update(entry);
        await catalogRepo.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static CatalogEntryResponse MapToResponse(CatalogEntry c) => new()
    {
        Id = c.Id,
        Type = c.Type.ToString(),
        CampaignId = c.CampaignId,
        IsGlobal = c.CampaignId is null,
        Name = c.Name,
        DataJson = c.DataJson,
        CreatedByGameMasterId = c.CreatedByGameMasterId,
        CreatedAt = c.CreatedAt,
        IsArchived = c.IsArchived,
        IsPublic = c.IsPublic
    };
}
