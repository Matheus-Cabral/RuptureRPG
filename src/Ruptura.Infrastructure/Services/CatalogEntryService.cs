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
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<CatalogEntryType>(type, out var parsedType) || !Enum.IsDefined(parsedType))
            return Result.Failure<IEnumerable<CatalogEntryResponse>>(ErrorCodes.Catalog.InvalidType);

        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null)
            return Result.Failure<IEnumerable<CatalogEntryResponse>>(ErrorCodes.Catalog.NotFound);

        var isMember = campaign.GameMasterId == callerId
            || await membershipRepo.ExistsAsync(campaignId, callerId, ct);
        if (!isMember)
            return Result.Failure<IEnumerable<CatalogEntryResponse>>(ErrorCodes.Catalog.NotFound);

        var entries = await catalogRepo.GetByTypeAsync(parsedType, campaignId, ct);
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

        if (entry.CampaignId is null)
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.CannotModifyGlobalEntry);

        var campaign = await campaignRepo.GetByIdAsync(entry.CampaignId.Value, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.NotFound);

        if (!string.Equals(entry.Name, request.Name, StringComparison.Ordinal)
            && (await catalogRepo.ExistsAsync(entry.Type, entry.CampaignId, request.Name, ct)
                || await catalogRepo.ExistsAsync(entry.Type, null, request.Name, ct)))
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.AlreadyExists);

        entry.Name = request.Name;
        entry.DataJson = request.DataJson;
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

        var campaign = await campaignRepo.GetByIdAsync(entry.CampaignId.Value, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure(ErrorCodes.Catalog.NotFound);

        catalogRepo.Remove(entry);
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
        CreatedAt = c.CreatedAt
    };
}
