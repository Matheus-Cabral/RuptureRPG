using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Journal;

namespace Ruptura.Infrastructure.Services;

public class JournalEntryService(
    ICharacterJournalEntryRepository journalRepo,
    ICharacterSheetRepository sheetRepo,
    ICampaignRepository campaignRepo,
    IFileStorageService fileStorage) : IJournalEntryService
{
    public async Task<Result<JournalEntryResponse>> CreateAsync(
        Guid callerId,
        Guid characterSheetId,
        CreateJournalEntryRequest request,
        CancellationToken ct = default)
    {
        var sheet = await sheetRepo.GetByIdAsync(characterSheetId, ct);
        if (sheet is null || sheet.OwnerId != callerId)
            return Result.Failure<JournalEntryResponse>(ErrorCodes.Journal.NotFound);

        var entry = new CharacterJournalEntry
        {
            Id = Guid.NewGuid(),
            CharacterSheetId = characterSheetId,
            Text = request.Text,
            ImagePaths = [],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await journalRepo.AddAsync(entry, ct);
        await journalRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(entry));
    }

    public async Task<Result<IEnumerable<JournalEntryResponse>>> GetByCharacterSheetAsync(
        Guid callerId,
        Guid characterSheetId,
        CancellationToken ct = default)
    {
        var sheet = await sheetRepo.GetByIdAsync(characterSheetId, ct);
        if (sheet is null)
            return Result.Failure<IEnumerable<JournalEntryResponse>>(ErrorCodes.Journal.NotFound);

        var campaign = await campaignRepo.GetByIdAsync(sheet.CampaignId, ct);
        var authorized = sheet.OwnerId == callerId || campaign?.GameMasterId == callerId;
        if (!authorized)
            return Result.Failure<IEnumerable<JournalEntryResponse>>(ErrorCodes.Journal.NotFound);

        var entries = await journalRepo.GetByCharacterSheetAsync(characterSheetId, ct);
        return Result.Success(entries.Select(MapToResponse));
    }

    public async Task<Result<CharacterJournalEntry>> AuthorizeReadAsync(
        Guid callerId, Guid entryId, CancellationToken ct = default)
    {
        var entry = await journalRepo.GetByIdAsync(entryId, ct);
        if (entry is null)
            return Result.Failure<CharacterJournalEntry>(ErrorCodes.Journal.NotFound);

        var sheet = await sheetRepo.GetByIdAsync(entry.CharacterSheetId, ct);
        if (sheet is null)
            return Result.Failure<CharacterJournalEntry>(ErrorCodes.Journal.NotFound);

        var campaign = await campaignRepo.GetByIdAsync(sheet.CampaignId, ct);
        var authorized = sheet.OwnerId == callerId || campaign?.GameMasterId == callerId;
        if (!authorized)
            return Result.Failure<CharacterJournalEntry>(ErrorCodes.Journal.NotFound);

        return Result.Success(entry);
    }

    public async Task<Result<CharacterJournalEntry>> AuthorizeWriteAsync(
        Guid callerId, Guid entryId, CancellationToken ct = default)
    {
        var entry = await journalRepo.GetByIdAsync(entryId, ct);
        if (entry is null)
            return Result.Failure<CharacterJournalEntry>(ErrorCodes.Journal.NotFound);

        var sheet = await sheetRepo.GetByIdAsync(entry.CharacterSheetId, ct);
        if (sheet is null || sheet.OwnerId != callerId)
            return Result.Failure<CharacterJournalEntry>(ErrorCodes.Journal.NotFound);

        return Result.Success(entry);
    }

    public Task<Result<JournalEntryResponse>> UpdateAsync(
        Guid callerId, Guid entryId, UpdateJournalEntryRequest request, CancellationToken ct = default) =>
        throw new NotImplementedException("Implemented in Task 5.");

    public Task<Result> DeleteAsync(Guid callerId, Guid entryId, CancellationToken ct = default) =>
        throw new NotImplementedException("Implemented in Task 5.");

    public Task<Result> AppendImagePathAsync(Guid entryId, string path, CancellationToken ct = default) =>
        throw new NotImplementedException("Implemented in Task 6.");

    // ── Private helpers ───────────────────────────────────────────────────────

    private static JournalEntryResponse MapToResponse(CharacterJournalEntry e) => new()
    {
        Id = e.Id,
        CharacterSheetId = e.CharacterSheetId,
        Text = e.Text,
        ImagePaths = e.ImagePaths,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
