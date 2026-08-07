using Ruptura.Domain.Enums;

namespace Ruptura.Domain.Entities;

public class Expedition
{
    public Guid Id { get; set; }
    public Guid GuildSheetId { get; set; }
    public ExpeditionKind Kind { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Participants { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Losses { get; set; } = string.Empty;
    public string ResourcesGained { get; set; } = string.Empty;
}
