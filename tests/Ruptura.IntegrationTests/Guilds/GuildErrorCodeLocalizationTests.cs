using System.Globalization;
using System.Reflection;
using System.Resources;
using FluentAssertions;
using Ruptura.API.Resources;
using Ruptura.Application.Common;

namespace Ruptura.IntegrationTests.Guilds;

// Guards against the "code shipped ahead of its string" class of bug: every public const string on
// ErrorCodes.Guild must resolve to a real value in BOTH the English (neutral) and pt-BR resources.
// Uses a ResourceManager bound to the SharedResources embedded resource (neutral base name
// "Ruptura.API.Resources.SharedResources" + the pt-BR satellite) — deterministic, no DI/culture
// base-name quirks.
public class GuildErrorCodeLocalizationTests
{
    private static readonly ResourceManager Resources =
        new("Ruptura.API.Resources.SharedResources", typeof(SharedResources).Assembly);

    public static IEnumerable<object[]> GuildErrorCodes() =>
        typeof(ErrorCodes.Guild)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
            .Select(f => new object[] { (string)f.GetValue(null)! });

    [Theory]
    [MemberData(nameof(GuildErrorCodes))]
    public void EveryGuildErrorCode_Resolves_InEnglishAndPortuguese(string code)
    {
        // tryParents:false reads each culture's own set — so a pt-BR key that is only present because
        // it falls back to the neutral resource does NOT count as a real pt-BR translation.
        var english = Resources
            .GetResourceSet(CultureInfo.InvariantCulture, createIfNotExists: true, tryParents: false)!
            .GetString(code);
        english.Should().NotBeNullOrWhiteSpace(
            "error code '{0}' must have an English (neutral) resource string", code);

        var portuguese = Resources
            .GetResourceSet(CultureInfo.GetCultureInfo("pt-BR"), createIfNotExists: true, tryParents: false)!
            .GetString(code);
        portuguese.Should().NotBeNullOrWhiteSpace(
            "error code '{0}' must have a pt-BR resource string", code);
    }
}
