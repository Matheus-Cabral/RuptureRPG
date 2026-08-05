using System.Text.Json;
using FluentAssertions;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.UnitTests.Application;

public class CharacterSheetDataSerializationTests
{
    [Fact]
    public void CharacterSheetData_RoundTripsThroughJson_PreservingAllModules()
    {
        var original = new CharacterSheetData
        {
            Identity = new CharacterIdentity
            {
                OriginId = Guid.NewGuid(),
                AptitudeIds = [Guid.NewGuid(), Guid.NewGuid()],
                PatronDisplayName = "Dom Alric"
            },
            Attributes = new CharacterAttributes { Corpo = 3, Controle = 4 },
            Combat = new CharacterCombat { CurrentHp = 12, ActiveConditions = ["Ferido"] },
            Skills = [new CharacterSkillEntry { CatalogEntryId = Guid.NewGuid(), Points = 30 }],
            Talents = [new CharacterCatalogRefEntry { CatalogEntryId = Guid.NewGuid() }],
            Equipment =
            [
                new CharacterEquipmentEntry
                {
                    CatalogEntryId = Guid.NewGuid(), Quantity = 1, IsEquipped = true,
                    LinkedSkillEntryId = Guid.NewGuid()
                }
            ],
            GuildRegistry = new CharacterGuildRegistry { Ranking = "Aço", Expeditions = 2 }
        };

        var json = JsonSerializer.Serialize(original);
        var roundTripped = JsonSerializer.Deserialize<CharacterSheetData>(json);

        roundTripped.Should().NotBeNull();
        roundTripped!.Identity.PatronDisplayName.Should().Be("Dom Alric");
        roundTripped.Identity.AptitudeIds.Should().HaveCount(2);
        roundTripped.Attributes.Corpo.Should().Be(3);
        roundTripped.Combat.ActiveConditions.Should().ContainSingle().Which.Should().Be("Ferido");
        roundTripped.Skills.Should().ContainSingle().Which.Points.Should().Be(30);
        roundTripped.Equipment.Should().ContainSingle().Which.IsEquipped.Should().BeTrue();
        roundTripped.GuildRegistry.Ranking.Should().Be("Aço");
    }

    [Fact]
    public void SkillCatalogData_DeserializesExistingSeedJsonShape()
    {
        const string json = """{"Area":"Combate — Armas","RelatedAttribute":"Controle"}""";

        var data = JsonSerializer.Deserialize<Ruptura.Shared.Catalog.SkillCatalogData>(json);

        data.Should().NotBeNull();
        data!.RelatedAttribute.Should().Be("Controle");
    }
}
