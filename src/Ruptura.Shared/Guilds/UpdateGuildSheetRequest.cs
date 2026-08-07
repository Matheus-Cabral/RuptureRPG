using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Guilds;

public class UpdateGuildSheetRequest
{
    [Required, MinLength(1), MaxLength(120)]
    public string GuildName { get; set; } = string.Empty;

    [Required]
    public string DataJson { get; set; } = "{}";

    // xmin concurrency token the client last read (GuildSheetResponse.Version). Enforced on save.
    public uint Version { get; set; }
}
