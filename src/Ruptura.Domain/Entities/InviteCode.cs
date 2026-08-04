namespace Ruptura.Domain.Entities;

public class InviteCode
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid CreatedByGameMasterId { get; set; }
    public Guid? UsedByPlayerId { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed => UsedByPlayerId.HasValue;

    public bool IsValid() => !IsUsed && ExpiresAt > DateTime.UtcNow;
}
