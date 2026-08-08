using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Guilds;

public class UpdateStaffRequest
{
    [Required]
    public string Kind { get; set; } = string.Empty;          // "Worker" | "Mercenary"
    [Required, MaxLength(80)]
    public string TypeOrRanking { get; set; } = string.Empty;
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;
    [Range(0, int.MaxValue)]
    public int DailySalary { get; set; }
    public bool IsActive { get; set; }
    public int? Efficiency { get; set; }
    public int? Morale { get; set; }
}
