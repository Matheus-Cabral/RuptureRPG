namespace Ruptura.Shared.Catalog;

public class CatalogEntryResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid? CampaignId { get; set; }
    public bool IsGlobal { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataJson { get; set; } = "{}";
    public Guid? CreatedByGameMasterId { get; set; }
    public DateTime CreatedAt { get; set; }
}
