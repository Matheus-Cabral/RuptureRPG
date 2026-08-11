namespace Ruptura.Shared.Bestiary;

public class NpcResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsOfficial { get; set; }          // true = system-owned example (read-only)
    public NpcData Data { get; set; } = new();
}
