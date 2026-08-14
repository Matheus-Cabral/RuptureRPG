namespace Ruptura.Web.Services;

public enum ManualType
{
    Player,
    GameMaster
}

/// <summary>
/// Pure mapping from (manual, language) to the Markdown file name served under
/// wwwroot/content/manuals/ — see docs/superpowers/specs/2026-08-14-manuals-page-design.md.
/// </summary>
public static class ManualReference
{
    public static string FileNameFor(ManualType type, string culture)
    {
        var baseName = type switch
        {
            ManualType.Player => "Manual_do_Jogador",
            ManualType.GameMaster => "Manual_do_Mestre",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        // LanguageSwitcher only ever stores exactly "en" or "pt-BR" (Layout/LanguageSwitcher.razor) —
        // match that literally rather than parsing/normalizing a general BCP-47 tag.
        var suffix = string.Equals(culture, "en", StringComparison.OrdinalIgnoreCase) ? ".en" : string.Empty;
        return $"{baseName}{suffix}.md";
    }
}
