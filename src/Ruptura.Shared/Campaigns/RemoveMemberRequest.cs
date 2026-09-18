using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Campaigns;

public class RemoveMemberRequest
{
    /// <summary>The calling Game Master's own login password, as confirmation of the removal.</summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}
