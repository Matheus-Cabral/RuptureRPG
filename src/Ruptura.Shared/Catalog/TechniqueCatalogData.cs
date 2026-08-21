using System.Text.Json.Serialization;

namespace Ruptura.Shared.Catalog;

public class TechniqueCatalogData
{
    public string Style { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    [JsonConverter(typeof(StringOrNumberJsonConverter))]
    public string PaCost { get; set; } = string.Empty;
    public string Damage { get; set; } = string.Empty;
    public string Effect { get; set; } = string.Empty;
    public string PowerTier { get; set; } = string.Empty; // "comum" | "avançada" | "suprema" — GDD §6.8 Habilidade weight
}
