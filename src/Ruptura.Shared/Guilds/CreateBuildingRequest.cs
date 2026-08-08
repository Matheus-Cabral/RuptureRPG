using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Guilds;

public class CreateBuildingRequest
{
    [Required]
    public Guid CatalogEntryId { get; set; }   // an Installation catalog entry
    public int Level { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}
