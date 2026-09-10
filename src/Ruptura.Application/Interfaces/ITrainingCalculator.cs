using Ruptura.Domain.Entities;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.Application.Interfaces;

public interface ITrainingCalculator
{
    TrainingProjection Project(
        Guid skillCatalogEntryId, string skillName, string skillArea, int currentPoints,
        IReadOnlyList<GuildBuilding> buildings, IReadOnlyList<GuildStaff> staff,
        Guid characterSheetId, int days, string correlation);
}
