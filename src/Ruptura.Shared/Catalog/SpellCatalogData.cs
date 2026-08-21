namespace Ruptura.Shared.Catalog;

public class SpellCatalogData
{
    public string School { get; set; } = string.Empty;
    public string ComplexityPaCost { get; set; } = string.Empty;
    public string Range { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Test { get; set; } = string.Empty;
    public string Damage { get; set; } = string.Empty;
    public string Effect { get; set; } = string.Empty;
    public string PowerTier { get; set; } = string.Empty; // "comum" | "avançada" | "suprema" — GDD §6.8 Habilidade weight
}
