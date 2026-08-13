namespace Ruptura.Shared.Content;

public class CreateArcRequest
{
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public ArcData Data { get; set; } = new();
}

public class UpdateArcRequest
{
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public ArcData Data { get; set; } = new();
}

public class CreateFloorRequest
{
    public Guid ArcId { get; set; }
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public FloorData Data { get; set; } = new();
}

public class UpdateFloorRequest
{
    public Guid ArcId { get; set; }
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public FloorData Data { get; set; } = new();
}

public class CreateSessionLogRequest
{
    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public SessionLogData Data { get; set; } = new();
}

public class UpdateSessionLogRequest
{
    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public SessionLogData Data { get; set; } = new();
}
