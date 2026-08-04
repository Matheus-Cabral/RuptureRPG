using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Catalog;

public class CreateCatalogEntryRequest
{
    [Required]
    public Guid CampaignId { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string DataJson { get; set; } = "{}";
}
