using Ruptura.Domain.Enums;

namespace Ruptura.Domain.Entities;

public class CraftingOrder
{
    public Guid Id { get; set; }
    public Guid GuildSheetId { get; set; }
    public CraftingCategory Category { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;      // Comum|Superior|Raro|Épico|Lendário|Divino
    public int ProgressDays { get; set; }
    public int RequiredDays { get; set; }
    public CraftingStatus Status { get; set; } = CraftingStatus.EmAndamento;
}
