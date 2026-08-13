namespace Ruptura.Shared.Combat;

// Server-authoritative combat tracker read model: echoes the session name/active flag and
// the live CombatState, plus the server-derived pressure (Pressure and its PressureStateKey
// are computed from the roster, never trusted from the client).
public class CombatSessionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public CombatState State { get; set; } = new();

    public int Pressure { get; set; }
    public string PressureStateKey { get; set; } = string.Empty;
}
