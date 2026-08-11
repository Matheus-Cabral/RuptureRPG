namespace Ruptura.Shared.Bestiary;

public class CreatureResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsOfficial { get; set; }          // true = system-owned example (read-only)
    public CreatureData Data { get; set; } = new();

    // Server-authoritative NP (§9.5.5) — the client's NP is never trusted.
    public int DerivedNp { get; set; }

    // Selected Category's advisory range (§9.5.6) + overflow flag (Regra do Teto).
    public int CategoryNpMin { get; set; }
    public int CategoryNpMax { get; set; }
    public bool CategoryOverflow { get; set; }
}
