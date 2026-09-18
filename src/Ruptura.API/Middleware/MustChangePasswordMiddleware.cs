using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Infrastructure.Services;
using Ruptura.Shared.Common;

namespace Ruptura.API.Middleware;

/// <summary>
/// While an account is on a GM-issued temporary password (the access token carries the
/// <see cref="JwtService.MustChangePasswordClaim"/> claim), only the endpoints needed to pick a
/// new password — or to identify/leave the session — are reachable. Everything else gets a 403,
/// so the forced change can't be bypassed by ignoring the client-side popup.
/// Must run after <c>UseAuthentication()</c>.
/// </summary>
public class MustChangePasswordMiddleware(RequestDelegate next)
{
    private static readonly string[] AllowedPaths =
    [
        "/api/auth/change-password",
        "/api/auth/me",
        "/api/auth/revoke"
    ];

    public async Task InvokeAsync(HttpContext context, IStringLocalizer<SharedResources> localizer)
    {
        var mustChange = context.User.Identity?.IsAuthenticated == true
            && context.User.HasClaim(JwtService.MustChangePasswordClaim, "true");

        if (mustChange && !IsAllowed(context.Request.Path))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(
                ApiResponse.Fail(localizer["Auth.PasswordChangeRequired"]));
            return;
        }

        await next(context);
    }

    private static bool IsAllowed(PathString path) =>
        AllowedPaths.Any(allowed => path.Equals(allowed, StringComparison.OrdinalIgnoreCase));
}
