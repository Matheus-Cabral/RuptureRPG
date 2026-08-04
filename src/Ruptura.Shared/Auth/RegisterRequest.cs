using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Auth;

public class RegisterRequest
{
    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class RegisterPlayerRequest : RegisterRequest
{
    [Required]
    public string InviteCode { get; set; } = string.Empty;
}
