namespace Ruptura.Shared.Campaigns;

public class ResetPlayerPasswordResponse
{
    /// <summary>Plain-text temporary password. Returned once and never stored in plain text.</summary>
    public string TemporaryPassword { get; set; } = string.Empty;
}
