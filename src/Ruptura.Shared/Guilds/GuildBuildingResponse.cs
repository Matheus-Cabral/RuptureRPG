namespace Ruptura.Shared.Guilds;

public class GuildBuildingResponse
{
    public Guid Id { get; set; }
    public Guid CatalogEntryId { get; set; }
    public string InstallationName { get; set; } = string.Empty; // resolved from the catalog for display
    public int Level { get; set; }
    public bool IsActive { get; set; }
}
