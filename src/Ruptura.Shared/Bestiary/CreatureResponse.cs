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

    // Open-ended top tiers (Chefe de Arco, Entidade Superior) have NO ceiling: null here means
    // "no upper bound". The calculator's internal sentinel (int.MaxValue) is deliberately mapped
    // to null at this response boundary so consumers (GM-2) never do arithmetic on 2147483647.
    public int? CategoryNpMax { get; set; }

    // Soft advisory only: true when DerivedNp exceeds the category ceiling by MORE than +15%
    // (§9.5.6 Regra do Teto). It does NOT flag under-NpMin, and is always false for open-ended
    // categories (CategoryNpMax == null).
    public bool CategoryOverflow { get; set; }
}
