namespace Ruptura.Shared;

/// <summary>
/// The 8 GDD character/mercenary ranks in ascending order (§ Ranking). Single source of truth —
/// used by the character ranking picker, the guild mercenary-salary reference, and the
/// rank-promotion detection order. Accented values are valid string literals.
/// </summary>
public static class RankProgression
{
    public static readonly IReadOnlyList<string> Ordered =
        ["Bronze", "Ferro", "Aço", "Prata", "Ouro", "Mithril", "Adamante", "Lendário"];
}
