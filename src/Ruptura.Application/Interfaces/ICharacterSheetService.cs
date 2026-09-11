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

    Task<Result<string>> GetRankingAsync(Guid sheetId, CancellationToken ct = default);

    Task<Result> SetRankingAsync(Guid sheetId, string ranking, CancellationToken ct = default);

    Task<Result<IEnumerable<CharacterSheetResponse>>> GetByCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default);

    Task<Result<CharacterSheetResponse>> GetMineAsync(
        Guid playerId, Guid campaignId, CancellationToken ct = default);

    Task<Result<CharacterSheetResponse>> UpdateAsync(
        Guid callerId, Guid sheetId, UpdateCharacterSheetRequest request, CancellationToken ct = default);

    Task<Result<TrainingProjection>> PreviewTrainingAsync(
        Guid callerId, Guid sheetId, Guid skillCatalogEntryId, int days, string correlation, CancellationToken ct = default);

    Task<Result<TechniqueProjectValidation>> ValidateTechniqueProjectStartAsync(
        Guid callerId, Guid sheetId, string category, Guid skillCatalogEntryId, CancellationToken ct = default);

    Task<Result<TechniqueProjectValidation>> ValidateCraftingProjectStartAsync(
        Guid callerId, Guid sheetId, Guid recipeCatalogEntryId, CancellationToken ct = default);
}
