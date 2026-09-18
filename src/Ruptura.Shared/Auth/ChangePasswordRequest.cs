using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Auth;

public class ChangePasswordRequest
{
    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
