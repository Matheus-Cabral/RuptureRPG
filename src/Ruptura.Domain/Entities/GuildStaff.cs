using Ruptura.Domain.Enums;

namespace Ruptura.Domain.Entities;

public class GuildStaff
{
    public Guid Id { get; set; }
    public Guid GuildSheetId { get; set; }
    public GuildStaffKind Kind { get; set; }
    public string TypeOrRanking { get; set; } = string.Empty; // worker type or merc ranking
    public string Name { get; set; } = string.Empty;
    public int DailySalary { get; set; }                      // pre-filled from GDD default, overridable
    public bool IsActive { get; set; } = true;
    public int? Efficiency { get; set; }                      // workers only, optional
    public int? Morale { get; set; }                          // workers only, optional

    // Dedicating an Instrutor to a character+área grants that character's Skill Training
    // calculator (Ruptura.Shared.CharacterSheets.TrainingProjection) a +1 pts/day Bônus de
    // Instrutor (GDD §6.4) when training a skill in that Área. Bare Guid?/string? — no FK,
    // matching the repo's soft-reference convention.
    public Guid? DedicatedCharacterSheetId { get; set; }
    public string? DedicatedSkillArea { get; set; }        // one of TrainingReference.AreaNames
}
