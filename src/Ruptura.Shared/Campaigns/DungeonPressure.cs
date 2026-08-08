namespace Ruptura.Shared.Campaigns;

// GDD Manual §4.2 — the Pressão counter (0-100) maps to a state + PE multiplier.
// State keys are unaccented resx-key suffixes (Dashboard.Pressure.<StateKey>); the UI localizes.
public static class DungeonPressure
{
    public static (string StateKey, decimal PeMultiplier) StateFor(int pressure) => pressure switch
    {
        >= 90 => ("Colapso", 1.50m),
        >= 60 => ("Critico", 1.25m),
        >= 25 => ("Agravado", 1.10m),
        _ => ("Estavel", 1.00m),
    };
}
