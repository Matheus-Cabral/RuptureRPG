namespace Ruptura.Shared.Bestiary;

// Typed blob stored in Creature.DataJson. String-only enum-like fields keep
// Ruptura.Shared free of project references. Validated against BestiaryReference sets
// on the server; the NP calculator is the authority over derived stats.
public class CreatureData
{
    // Classification (§9.5.1-9.5.3, 9.5.6).
    public string Type { get; set; } = string.Empty;        // free / homebrew (§9.5.9)
    public string Function { get; set; } = string.Empty;    // BestiaryReference.Functions (custom allowed)
    public string Behavior { get; set; } = string.Empty;    // BestiaryReference.Behaviors (fixed set)
    public string Category { get; set; } = string.Empty;    // BestiaryReference.Categories (fixed set)

    // NP inputs.
    public CreatureAttributes Attributes { get; set; } = new();
    public List<CreatureNaturalSkill> NaturalSkills { get; set; } = [];
    public List<CreatureCharacteristic> Characteristics { get; set; } = [];
    public List<CreatureAbility> Abilities { get; set; } = [];
    public List<CreatureEquipment> Equipment { get; set; } = [];

    // Authored combat sheet (§9.5.7).
    public int Pv { get; set; }
    public int DefesaPassiva { get; set; }
    public int Deslocamento { get; set; }
    public string AtaquePrincipal { get; set; } = string.Empty;  // e.g. "2d10+5 vs Defesa"
    public string Dano { get; set; } = string.Empty;             // e.g. "2d6+3"

    // Balance & rewards.
    public string Fraqueza { get; set; } = string.Empty;         // required (Regra da Fraqueza)
    public List<string> Recompensas { get; set; } = [];          // feeds GM-3
    public string Notes { get; set; } = string.Empty;
}

// The 8 GDD attributes as int scores (default 1 = grade bonus 0).
public class CreatureAttributes
{
    public int Corpo { get; set; } = 1;
    public int Controle { get; set; } = 1;
    public int Vigor { get; set; } = 1;
    public int Presenca { get; set; } = 1;
    public int Intelecto { get; set; } = 1;
    public int Percepcao { get; set; } = 1;
    public int Vontade { get; set; } = 1;
    public int Afinidade { get; set; } = 1;
}

public class CreatureNaturalSkill
{
    public string Name { get; set; } = string.Empty;
    public int Points { get; set; }
}

public class CreatureCharacteristic
{
    public string Name { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;   // BestiaryReference.Weights (Menor|Media|Maior|Suprema)
}

public class CreatureAbility
{
    public string Name { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;     // BestiaryReference.Tiers (Comum|Avancada|Suprema)
}

public class CreatureEquipment
{
    public string Name { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;   // BestiaryReference.Rarities (Comum..Divino)
}
