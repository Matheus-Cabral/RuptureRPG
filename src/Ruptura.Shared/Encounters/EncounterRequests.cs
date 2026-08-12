namespace Ruptura.Shared.Encounters;

public class CreateEncounterRequest
{
    public string Name { get; set; } = string.Empty;
    public EncounterData Data { get; set; } = new();
}

public class UpdateEncounterRequest
{
    public string Name { get; set; } = string.Empty;
    public EncounterData Data { get; set; } = new();
}
