using System.Globalization;
using System.Reflection;
using System.Resources;
using FluentAssertions;
using Ruptura.API.Resources;
using Ruptura.Application.Common;

namespace Ruptura.IntegrationTests.Content;

// Guards against the "code shipped ahead of its string" class of bug: every public const string on
// ErrorCodes.Session must resolve to a real value in BOTH the English (neutral) and pt-BR resources.
// Mirrors ContentErrorCodeLocalizationTests — a ResourceManager bound to the SharedResources embedded
// resource, read per-culture with tryParents:false so a pt-BR key present only via neutral fallback
// does not count as a real translation.
public class SessionErrorCodeLocalizationTests
{
    private static readonly ResourceManager Resources =
        new("Ruptura.API.Resources.SharedResources", typeof(SharedResources).Assembly);

    public static IEnumerable<object[]> SessionErrorCodes() =>
        typeof(ErrorCodes.Session)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
            .Select(f => new object[] { (string)f.GetValue(null)! });

    [Theory]
    [MemberData(nameof(SessionErrorCodes))]
    public void EverySessionErrorCode_Resolves_InEnglishAndPortuguese(string code)
    {
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
