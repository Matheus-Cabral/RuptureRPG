namespace Ruptura.Shared.Combat;

// Single source of truth for the combat tracker's fixed picker sets. Consumed by the
// write-side validators and the UI toggles. String-only to keep Ruptura.Shared free of
// project references.
public static class CombatReference
{
    // Combatant conditions — the fixed set offered as toggles and validated on write.
    public static readonly IReadOnlyList<string> Conditions =
    [
        "FeridoLeve", "FeridoGrave", "Sangrando", "Atordoado", "Enfraquecido",
        "Amedrontado", "Imobilizado", "Agonizante", "Morto"
    ];

    // Combatant kinds — where a combatant came from.
    public static readonly IReadOnlyList<string> Kinds =
    [
        "Character", "Creature", "Adhoc"
    ];
}
