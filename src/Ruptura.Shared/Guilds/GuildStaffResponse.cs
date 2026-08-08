namespace Ruptura.Shared.Guilds;

public class GuildStaffResponse
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = string.Empty;          // "Worker" | "Mercenary"
    public string TypeOrRanking { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DailySalary { get; set; }
    public bool IsActive { get; set; }
    public int? Efficiency { get; set; }
    public int? Morale { get; set; }
}
