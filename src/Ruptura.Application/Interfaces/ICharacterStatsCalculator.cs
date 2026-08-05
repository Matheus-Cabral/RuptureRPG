using Ruptura.Domain.Entities;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.Application.Interfaces;

public interface ICharacterStatsCalculator
{
    CharacterDerivedStats Calculate(CharacterSheetData data, IReadOnlyDictionary<Guid, CatalogEntry> catalogEntries);
}
