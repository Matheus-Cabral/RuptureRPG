using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Campaigns;

public class AssignMemberRequest
{
    [Required]
    public Guid PlayerId { get; set; }
}
