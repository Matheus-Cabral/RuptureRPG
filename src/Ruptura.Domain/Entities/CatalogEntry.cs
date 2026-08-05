using Ruptura.Domain.Enums;

namespace Ruptura.Domain.Entities;

public class CatalogEntry
{
    public Guid Id { get; set; }
    public CatalogEntryType Type { get; set; }
    public Guid? CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataJson { get; set; } = "{}";
    public bool IsArchived { get; set; }
    public Guid? CreatedByGameMasterId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
