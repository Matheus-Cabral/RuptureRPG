using System.Globalization;
using System.Reflection;
using System.Resources;
using FluentAssertions;
using Ruptura.API.Resources;
using Ruptura.Application.Common;

namespace Ruptura.IntegrationTests.Encounters;

// Guards against the "code shipped ahead of its string" class of bug: every public const string on
// ErrorCodes.Encounter must resolve to a real value in BOTH the English (neutral) and pt-BR resources.
// Mirrors BestiaryErrorCodeLocalizationTests — a ResourceManager bound to the SharedResources embedded
// resource, read per-culture with tryParents:false so a pt-BR key present only via neutral fallback
// does not count as a real translation.
public class EncounterErrorCodeLocalizationTests
{
    private static readonly ResourceManager Resources =
        new("Ruptura.API.Resources.SharedResources", typeof(SharedResources).Assembly);

    public static IEnumerable<object[]> EncounterErrorCodes() =>
        typeof(ErrorCodes.Encounter)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
            .Select(f => new object[] { (string)f.GetValue(null)! });

    [Theory]
    [MemberData(nameof(EncounterErrorCodes))]
    public void EveryEncounterErrorCode_Resolves_InEnglishAndPortuguese(string code)
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
