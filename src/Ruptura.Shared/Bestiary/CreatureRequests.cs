namespace Ruptura.Shared.Bestiary;

public class CreateCreatureRequest
{
    public string Name { get; set; } = string.Empty;
    public CreatureData Data { get; set; } = new();
}

public class UpdateCreatureRequest
{
    public string Name { get; set; } = string.Empty;
    public CreatureData Data { get; set; } = new();
}
