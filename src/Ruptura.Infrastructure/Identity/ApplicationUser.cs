using Microsoft.AspNetCore.Identity;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? RecruitedByGameMasterId { get; set; }

    /// <summary>True while the account is using a GM-issued temporary password.</summary>
    public bool MustChangePassword { get; set; }
}
