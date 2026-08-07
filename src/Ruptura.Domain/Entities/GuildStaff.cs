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
}
