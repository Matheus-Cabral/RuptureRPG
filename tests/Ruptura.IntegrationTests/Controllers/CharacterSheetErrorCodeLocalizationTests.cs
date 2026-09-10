using System.Globalization;
using System.Reflection;
using System.Resources;
using FluentAssertions;
using Ruptura.API.Resources;
using Ruptura.Application.Common;

namespace Ruptura.IntegrationTests.Controllers;

public class CharacterSheetErrorCodeLocalizationTests
{
    private static readonly ResourceManager Resources =
        new("Ruptura.API.Resources.SharedResources", typeof(SharedResources).Assembly);

    public static IEnumerable<object[]> CharacterSheetErrorCodes() =>
        typeof(ErrorCodes.CharacterSheet)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
            .Select(f => new object[] { (string)f.GetValue(null)! });

    [Theory]
    [MemberData(nameof(CharacterSheetErrorCodes))]
    public void EveryCharacterSheetErrorCode_Resolves_InEnglishAndPortuguese(string code)
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
