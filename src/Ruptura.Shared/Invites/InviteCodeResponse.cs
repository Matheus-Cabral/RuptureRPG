namespace Ruptura.Shared.Invites;

public class InviteCodeResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }

    // Only populated for the GM's own invite list (GetByGameMasterAsync) — never
    // returned from the anonymous code-validation endpoint, to avoid leaking a
    // player's name/email to an unauthenticated caller who knows/guesses a code.
    public string? RedeemedByDisplayName { get; set; }
    public string? RedeemedByEmail { get; set; }
}
