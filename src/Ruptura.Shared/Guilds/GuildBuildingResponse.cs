namespace Ruptura.Shared.Guilds;

public class GuildBuildingResponse
{
    public Guid Id { get; set; }
    public Guid CatalogEntryId { get; set; }
    public string InstallationName { get; set; } = string.Empty; // resolved from the catalog for display
    public int Level { get; set; }
    public bool IsActive { get; set; }

    // Read-time computed installation detail, resolved from the catalog entry's InstallationCatalogData.
    public string Category { get; set; } = string.Empty;
    public int Weight { get; set; }
    public int LevelCap { get; set; }
    public string Prerequisites { get; set; } = string.Empty;
    public string Unlocks { get; set; } = string.Empty;
}
