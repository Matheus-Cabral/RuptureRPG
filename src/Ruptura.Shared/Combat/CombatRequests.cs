namespace Ruptura.Shared.Combat;

public class CreateCombatSessionRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateCombatStateRequest
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public CombatState State { get; set; } = new();
}

public class StartFromEncounterRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid EncounterId { get; set; }
}
