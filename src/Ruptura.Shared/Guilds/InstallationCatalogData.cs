namespace Ruptura.Shared.Guilds;

public class InstallationCatalogData
{
    public string Category { get; set; } = string.Empty;   // Fundação|Produção|Especialização|Institucional|Monumental
    public int Weight { get; set; }
    public int LevelCap { get; set; }
    public string Prerequisites { get; set; } = string.Empty;
    public string Unlocks { get; set; } = string.Empty;
    public bool NonConstructible { get; set; }             // true only for Portão
}
