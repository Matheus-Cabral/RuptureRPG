namespace Ruptura.Shared.Bestiary;

public class CreateNpcRequest
{
    public string Name { get; set; } = string.Empty;
    public NpcData Data { get; set; } = new();
}

public class UpdateNpcRequest
{
    public string Name { get; set; } = string.Empty;
    public NpcData Data { get; set; } = new();
}
