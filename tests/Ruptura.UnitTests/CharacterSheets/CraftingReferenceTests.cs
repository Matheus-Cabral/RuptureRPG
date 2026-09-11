using FluentAssertions;
using Ruptura.Shared.CharacterSheets;
using Xunit;

namespace Ruptura.UnitTests.CharacterSheets;

public class CraftingReferenceTests
{
    [Theory]
    [MemberData(nameof(Rarities))]
    public void EveryCraftableRarity_HasAnEntryInRequiredDaysByRarity(string rarity) =>
        CraftingReference.RequiredDaysByRarity.Should().ContainKey(rarity);

    [Theory]
    [MemberData(nameof(Rarities))]
    public void EveryCraftableRarity_HasAnEntryInMaterialsCostByRarity(string rarity) =>
        CraftingReference.MaterialsCostByRarity.Should().ContainKey(rarity);

    [Theory]
    [MemberData(nameof(Rarities))]
    public void EveryCraftableRarity_HasAnEntryInInstallationByRarity(string rarity) =>
        CraftingReference.InstallationByRarity.Should().ContainKey(rarity);

    public static IEnumerable<object[]> Rarities() =>
        CraftingReference.CraftableRarities.Select(r => new object[] { r });

    [Theory]
    [InlineData("comum", true)]
    [InlineData("COMUM", true)]
    [InlineData(" Comum ", true)]
    [InlineData("Divino", false)]
    [InlineData("Homebrew", false)]
    public void IsCraftable_IsCaseAndWhitespaceInsensitive_AndExcludesDivino(string rarity, bool expected) =>
        CraftingReference.IsCraftable(rarity).Should().Be(expected);
}
