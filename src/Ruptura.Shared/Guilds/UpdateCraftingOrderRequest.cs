using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Guilds;

public class UpdateCraftingOrderRequest
{
    [Required]
    public string Category { get; set; } = string.Empty;
    [MaxLength(120)]
    public string ItemName { get; set; } = string.Empty;
    [MaxLength(40)]
    public string Quality { get; set; } = string.Empty;
    public int ProgressDays { get; set; }
    public int RequiredDays { get; set; }
    public string Status { get; set; } = string.Empty;
}
