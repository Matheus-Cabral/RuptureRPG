using Ruptura.Application.Common;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Journal;

namespace Ruptura.Application.Interfaces;

public interface IJournalEntryService
{
    Task<Result<JournalEntryResponse>> CreateAsync(
        Guid callerId, Guid characterSheetId, CreateJournalEntryRequest request, CancellationToken ct = default);

    Task<Result<IEnumerable<JournalEntryResponse>>> GetByCharacterSheetAsync(
        Guid callerId, Guid characterSheetId, CancellationToken ct = default);

    Task<Result<JournalEntryResponse>> UpdateAsync(
        Guid callerId, Guid entryId, UpdateJournalEntryRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid callerId, Guid entryId, CancellationToken ct = default);

    // Internal authorization primitives for MediaController (Task 8) — return the
    // raw entity, not a mapped response. AuthorizeReadAsync allows owner-or-GM;
    // AuthorizeWriteAsync allows owner only (per the design spec's permission
    // matrix: journal images can only be added by the sheet's owner).
    Task<Result<CharacterJournalEntry>> AuthorizeReadAsync(
        Guid callerId, Guid entryId, CancellationToken ct = default);

    Task<Result<CharacterJournalEntry>> AuthorizeWriteAsync(
        Guid callerId, Guid entryId, CancellationToken ct = default);

    Task<Result> AppendImagePathAsync(Guid entryId, string path, CancellationToken ct = default);
}
