namespace Ruptura.Shared.Guilds;

public class CraftingOrderResponse
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;   // Forja|Alquimia|Encantamento|Engenharia|Artefatos
    public string ItemName { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;    // Comum|Superior|Raro|Épico|Lendário|Divino
    public int ProgressDays { get; set; }
    public int RequiredDays { get; set; }
    public string Status { get; set; } = string.Empty;     // EmAndamento|Concluido|Cancelado
}
