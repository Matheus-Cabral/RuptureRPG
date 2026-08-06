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

    public async Task<Result<JournalEntryResponse>> UpdateAsync(
        Guid callerId,
        Guid entryId,
        UpdateJournalEntryRequest request,
        CancellationToken ct = default)
    {
        var authorized = await AuthorizeWriteAsync(callerId, entryId, ct);
        if (authorized.IsFailure)
            return Result.Failure<JournalEntryResponse>(authorized.Error!);

        var entry = authorized.Value!;
        // The client's ImagePaths is a SELECTOR over paths the entry already owns, never
        // a new filesystem address — the client can only ever remove paths here, never
        // introduce new ones (new paths only ever arrive via AppendImagePathAsync from
        // MediaController.Upload). Whatever ends up persisted must be a subset of what
        // was already there.
        var keptPaths = entry.ImagePaths.Intersect(request.ImagePaths).ToList();
        var droppedPaths = entry.ImagePaths.Except(keptPaths).ToList();
        foreach (var path in droppedPaths)
        {
            try
            {
                await fileStorage.DeleteAsync(path, ct);
            }
            catch (ArgumentException)
            {
                // A path that fails ResolveSafePath's validation was never a real file
                // this entry owned in the first place — nothing to delete.
            }
        }

        entry.Text = request.Text;
        entry.ImagePaths = keptPaths;
        entry.UpdatedAt = DateTime.UtcNow;

        journalRepo.Update(entry);
        await journalRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(entry));
    }

    public async Task<Result> DeleteAsync(Guid callerId, Guid entryId, CancellationToken ct = default)
    {
        var authorized = await AuthorizeWriteAsync(callerId, entryId, ct);
        if (authorized.IsFailure)
            return Result.Failure(authorized.Error!);

        var entry = authorized.Value!;
        foreach (var path in entry.ImagePaths)
            await fileStorage.DeleteAsync(path, ct);

        journalRepo.Remove(entry);
        await journalRepo.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> AppendImagePathAsync(Guid entryId, string path, CancellationToken ct = default)
    {
        var entry = await journalRepo.GetByIdAsync(entryId, ct);
        if (entry is null)
            return Result.Failure(ErrorCodes.Journal.NotFound);

        entry.ImagePaths = [.. entry.ImagePaths, path];
        entry.UpdatedAt = DateTime.UtcNow;

        journalRepo.Update(entry);
        await journalRepo.SaveChangesAsync(ct);

        return Result.Success();
    }

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
