using System.Text.Json;
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

        await sheetRepo.AddAsync(sheet, ct);
        await sheetRepo.SaveChangesAsync(ct);

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

    public Task<Result<CharacterSheetResponse>> UpdateAsync(Guid callerId, Guid sheetId, UpdateCharacterSheetRequest request, CancellationToken ct = default) =>
        throw new NotImplementedException("Implemented in Task 8.");

    // ── Private helpers (shared with Tasks 7-8) ─────────────────────────────

    private async Task<CharacterSheetResponse> MapToResponseAsync(CharacterSheet sheet, CancellationToken ct)
    {
        var data = JsonSerializer.Deserialize<CharacterSheetData>(sheet.DataJson) ?? new CharacterSheetData();
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
