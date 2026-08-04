using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Ruptura.Web.Auth;

public class JwtAuthStateProvider(ILocalStorageService localStorage) : AuthenticationStateProvider
{
    private const string AccessKey = "access_token";
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await localStorage.GetItemAsync<string>(AccessKey);
        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        var claims = ParseClaims(token);

        var expiry = claims.FirstOrDefault(c =>
            c.Type == JwtRegisteredClaimNames.Exp)?.Value;
        if (expiry is not null && long.TryParse(expiry, out var exp))
        {
            if (DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime <= DateTime.UtcNow)
                return Anonymous;
        }

        // Use "role" as roleClaimType so [Authorize(Roles="GameMaster")] works
        var identity = new ClaimsIdentity(claims, "jwt",
            nameType: JwtRegisteredClaimNames.Name,
            roleType: "role");

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyAuthStateChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
        return token.Claims;
    }
}
