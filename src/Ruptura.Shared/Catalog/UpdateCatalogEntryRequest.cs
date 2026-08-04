using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Catalog;

public class UpdateCatalogEntryRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string DataJson { get; set; } = "{}";
}
