using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.CharacterSheets;

public class GrantCharacterSheetRequest
{
    [Required]
    public Guid PlayerId { get; set; }

    [Required, MinLength(2), MaxLength(100)]
    public string CharacterName { get; set; } = string.Empty;
}
