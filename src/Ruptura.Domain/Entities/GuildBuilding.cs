namespace Ruptura.Domain.Entities;

// One built installation. CatalogEntryId references a CatalogEntry of type Installation.
public class GuildBuilding
{
    public Guid Id { get; set; }
    public Guid GuildSheetId { get; set; }
    public Guid CatalogEntryId { get; set; }
    public int Level { get; set; } = 1;
    public bool IsActive { get; set; } = true; // CS caps active buildings (§10.9)
}
