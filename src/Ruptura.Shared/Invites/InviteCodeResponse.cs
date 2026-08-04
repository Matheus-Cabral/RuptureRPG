namespace Ruptura.Shared.Invites;

public class InviteCodeResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
