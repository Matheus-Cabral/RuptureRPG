using Ruptura.Application.Common;
using Ruptura.Domain.Entities;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.Application.Interfaces;

public interface ICharacterSheetService
{
    Task<Result<CharacterSheetResponse>> CreateAsync(
        Guid gameMasterId, Guid campaignId, GrantCharacterSheetRequest request, CancellationToken ct = default);

    Task<Result<CharacterSheetResponse>> GetAsync(
        Guid callerId, Guid sheetId, CancellationToken ct = default);

    Task<Result<CharacterSheet>> AuthorizeAccessAsync(
        Guid callerId, Guid sheetId, CancellationToken ct = default);

    Task<Result> SetPortraitPathAsync(Guid sheetId, string? path, CancellationToken ct = default);

    Task<Result<IEnumerable<CharacterSheetResponse>>> GetByCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default);

    Task<Result<CharacterSheetResponse>> GetMineAsync(
        Guid playerId, Guid campaignId, CancellationToken ct = default);

    Task<Result<CharacterSheetResponse>> UpdateAsync(
        Guid callerId, Guid sheetId, UpdateCharacterSheetRequest request, CancellationToken ct = default);
}
