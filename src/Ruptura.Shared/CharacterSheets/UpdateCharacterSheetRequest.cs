using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.CharacterSheets;

public class UpdateCharacterSheetRequest
{
    [Required, MinLength(2), MaxLength(100)]
    public string CharacterName { get; set; } = string.Empty;

    [Required]
    public string DataJson { get; set; } = "{}";

    public bool IsDead { get; set; }
    public bool IsRetired { get; set; }
    public string? PortraitImagePath { get; set; }
}
