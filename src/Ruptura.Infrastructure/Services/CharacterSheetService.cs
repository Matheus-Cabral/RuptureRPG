using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.Infrastructure.Services;

public class CharacterSheetService(
    ICharacterSheetRepository sheetRepo,
    ICampaignRepository campaignRepo,
    ICampaignMembershipRepository membershipRepo,
    ICatalogEntryRepository catalogRepo,
    ICharacterStatsCalculator calculator) : ICharacterSheetService
{
    public async Task<Result<CharacterSheetResponse>> CreateAsync(
        Guid gameMasterId,
        Guid campaignId,
        GrantCharacterSheetRequest request,
        CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.NotFound);

        if (!await membershipRepo.ExistsAsync(campaignId, request.PlayerId, ct))
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.PlayerNotMember);

        var existingAlive = await sheetRepo.GetAliveByOwnerAndCampaignAsync(request.PlayerId, campaignId, ct);
        if (existingAlive is not null)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.AlreadyHasAliveCharacter);

        var sheet = new CharacterSheet
        {
            Id = Guid.NewGuid(),
            CharacterName = request.CharacterName,
            OwnerId = request.PlayerId,
            CampaignId = campaignId,
            GrantedByGameMasterId = gameMasterId,
            IsDead = false,
            IsRetired = false,
            DataJson = JsonSerializer.Serialize(new CharacterSheetData()),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            await sheetRepo.AddAsync(sheet, ct);
            await sheetRepo.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Only the alive-per-owner-per-campaign partial unique index is on this table,
            // so any DbUpdateException on this save path means that race — see design spec §4.1.
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.AlreadyHasAliveCharacter);
        }

        return Result.Success(await MapToResponseAsync(sheet, ct));
    }

    public async Task<Result<CharacterSheetResponse>> GetAsync(
        Guid callerId, Guid sheetId, CancellationToken ct = default)
    {
        var sheet = await sheetRepo.GetByIdAsync(sheetId, ct);
        if (sheet is null)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.NotFound);

        var campaign = await campaignRepo.GetByIdAsync(sheet.CampaignId, ct);
        var authorized = sheet.OwnerId == callerId || campaign?.GameMasterId == callerId;
        if (!authorized)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.NotFound);

        return Result.Success(await MapToResponseAsync(sheet, ct));
    }

    public async Task<Result<IEnumerable<CharacterSheetResponse>>> GetByCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<IEnumerable<CharacterSheetResponse>>(ErrorCodes.CharacterSheet.NotFound);

        var sheets = await sheetRepo.GetByCampaignAsync(campaignId, ct);
        var responses = new List<CharacterSheetResponse>();
        foreach (var sheet in sheets)
            responses.Add(await MapToResponseAsync(sheet, ct));

        return Result.Success(responses.AsEnumerable());
    }

    public async Task<Result<CharacterSheetResponse>> GetMineAsync(
        Guid playerId, Guid campaignId, CancellationToken ct = default)
    {
        var sheet = await sheetRepo.GetAliveByOwnerAndCampaignAsync(playerId, campaignId, ct);
        if (sheet is null)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.NotFound);

        return Result.Success(await MapToResponseAsync(sheet, ct));
    }

    public async Task<Result<CharacterSheetResponse>> UpdateAsync(
        Guid callerId, Guid sheetId, UpdateCharacterSheetRequest request, CancellationToken ct = default)
    {
        var sheet = await sheetRepo.GetByIdAsync(sheetId, ct);
        if (sheet is null)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.NotFound);

        var campaign = await campaignRepo.GetByIdAsync(sheet.CampaignId, ct);
        var isOwner = sheet.OwnerId == callerId;
        var isGameMaster = campaign?.GameMasterId == callerId;
        if (!isOwner && !isGameMaster)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.NotFound);

        var statusChanged = request.IsDead != sheet.IsDead || request.IsRetired != sheet.IsRetired;
        if (statusChanged && !isGameMaster)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.OnlyGameMasterCanChangeStatus);

        sheet.CharacterName = request.CharacterName;
        sheet.DataJson = request.DataJson;
        sheet.PortraitImagePath = request.PortraitImagePath;
        if (isGameMaster)
        {
            sheet.IsDead = request.IsDead;
            sheet.IsRetired = request.IsRetired;
        }
        sheet.UpdatedAt = DateTime.UtcNow;

        try
        {
            sheetRepo.Update(sheet);
            await sheetRepo.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Only the alive-per-owner-per-campaign partial unique index is on this table,
            // so any DbUpdateException on this save path means that race — see design spec §4.1.
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.AlreadyHasAliveCharacter);
        }

        return Result.Success(await MapToResponseAsync(sheet, ct));
    }

    // ── Private helpers (shared with Tasks 7-8) ─────────────────────────────

    private async Task<CharacterSheetResponse> MapToResponseAsync(CharacterSheet sheet, CancellationToken ct)
    {
        var data = DeserializeSheetData(sheet.DataJson);
        var referencedIds = CollectReferencedCatalogIds(data);
        var catalogEntries = referencedIds.Count == 0
            ? new Dictionary<Guid, CatalogEntry>()
            : (await catalogRepo.GetByIdsAsync(referencedIds, ct)).ToDictionary(e => e.Id);
        var derived = calculator.Calculate(data, catalogEntries);

        return new CharacterSheetResponse
        {
            Id = sheet.Id,
            CharacterName = sheet.CharacterName,
            OwnerId = sheet.OwnerId,
            CampaignId = sheet.CampaignId,
            GrantedByGameMasterId = sheet.GrantedByGameMasterId,
            IsDead = sheet.IsDead,
            IsRetired = sheet.IsRetired,
            PortraitImagePath = sheet.PortraitImagePath,
            Data = data,
            DerivedStats = derived,
            CreatedAt = sheet.CreatedAt,
            UpdatedAt = sheet.UpdatedAt
        };
    }

    // Defense-in-depth for any already-corrupt row (or a future write path that bypasses
    // UpdateCharacterSheetRequestValidator's DataJson rule) — never let a deserialization
    // failure propagate out of a read and 500 every subsequent GET of this sheet.
    private static CharacterSheetData DeserializeSheetData(string dataJson)
    {
        try
        {
            var data = JsonSerializer.Deserialize<CharacterSheetData>(dataJson) ?? new CharacterSheetData();
            return NormalizeSheetData(data);
        }
        catch (JsonException)
        {
            return new CharacterSheetData();
        }
    }

    // System.Text.Json ignores C#'s non-nullable annotations: valid JSON like
    // `{"Skills":null}` overwrites a property's `= []`/`= new()` initializer with an actual
    // null, even though CharacterSheetData declares it non-nullable. That JSON is not
    // reachable via the API today (UpdateCharacterSheetRequestValidator rejects it before
    // save), but a future write path that bypasses the validator (direct DB write,
    // migration, another endpoint) could still produce it. Restoring every such module to
    // its default here — right after deserialization, once — gives every downstream reader
    // (CollectReferencedCatalogIds, CharacterStatsCalculator.Calculate) a guaranteed-non-null
    // object instead of requiring each of them to re-guard the same properties.
    // AttributeTrial is intentionally excluded: it is legitimately nullable (Module 9 is
    // optional per its own design), so a null there is not corruption.
    private static CharacterSheetData NormalizeSheetData(CharacterSheetData data)
    {
        data.Identity ??= new();
        data.Identity.AptitudeIds ??= [];
        data.Attributes ??= new();
        data.Combat ??= new();
        data.Skills ??= [];
        data.Talents ??= [];
        data.Spells ??= [];
        data.Techniques ??= [];
        data.Equipment ??= [];
        data.Currency ??= new();
        data.GuildRegistry ??= new();
        return data;
    }

    private static List<Guid> CollectReferencedCatalogIds(CharacterSheetData data)
    {
        var ids = new List<Guid>();
        if (data.Identity.OriginId is { } origin) ids.Add(origin);
        if (data.Identity.BackgroundId is { } background) ids.Add(background);
        if (data.Identity.LineageId is { } lineage) ids.Add(lineage);
        ids.AddRange(data.Identity.AptitudeIds);
        if (data.Identity.InitialTalentId is { } initialTalent) ids.Add(initialTalent);
        ids.AddRange(data.Skills.Select(s => s.CatalogEntryId));
        ids.AddRange(data.Talents.Select(t => t.CatalogEntryId));
        ids.AddRange(data.Spells.Select(s => s.CatalogEntryId));
        ids.AddRange(data.Techniques.Select(t => t.CatalogEntryId));
        ids.AddRange(data.Equipment.Select(e => e.CatalogEntryId));
        ids.AddRange(data.Equipment.Where(e => e.LinkedSkillEntryId.HasValue).Select(e => e.LinkedSkillEntryId!.Value));
        return ids.Distinct().ToList();
    }
}
