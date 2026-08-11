using FluentAssertions;
using Ruptura.Application.Interfaces;
using Ruptura.Application.Services;
using Ruptura.Shared.Bestiary;

namespace Ruptura.UnitTests.Bestiary;

public class CreatureStatsCalculatorTests
{
    private readonly ICreatureStatsCalculator _sut = new CreatureStatsCalculator();

    private static CreatureData BaseCreature() => new()
    {
        Attributes = new CreatureAttributes(),   // all default 1 → (1-1)=0
        NaturalSkills = [],
        Characteristics = [],
        Abilities = [],
        Equipment = []
    };

    [Fact]
    public void Calculate_SumsAllFiveTerms()
    {
        var data = BaseCreature();
        data.Attributes.Corpo = 5;                                          // (5-1)=4
        data.NaturalSkills.Add(new CreatureNaturalSkill { Name = "Caçar", Points = 50 });   // grade bonus +2
        data.Characteristics.Add(new CreatureCharacteristic { Name = "Couraça", Weight = "Maior" });   // +5
        data.Abilities.Add(new CreatureAbility { Name = "Sopro", Tier = "Avancada" });      // +10
        data.Equipment.Add(new CreatureEquipment { Name = "Garra", Rarity = "Raro" });      // +7

        var result = _sut.Calculate(data);

        result.Np.Should().Be(4 + 2 + 5 + 10 + 7); // 28
    }

    [Fact]
    public void Calculate_AttributeGradeBonus_UsesScoreMinusOne()
    {
        var data = BaseCreature();
        data.Attributes.Corpo = 5;      // +4
        data.Attributes.Vigor = 3;      // +2

        var result = _sut.Calculate(data);

        result.Np.Should().Be(6);
    }

    [Theory]
    [InlineData(100, 4)]
    [InlineData(75, 3)]
    [InlineData(50, 2)]
    [InlineData(25, 1)]
    [InlineData(10, 0)]
    [InlineData(9, -2)]
    [InlineData(0, -2)]
    public void Calculate_NaturalSkillGradeBonus_MatchesTierLadder(int points, int expected)
    {
        var data = BaseCreature();
        data.NaturalSkills.Add(new CreatureNaturalSkill { Name = "S", Points = points });

        var result = _sut.Calculate(data);

        result.Np.Should().Be(expected);
    }

    [Theory]
    [InlineData("Menor", 1)]
    [InlineData("Media", 3)]
    [InlineData("Maior", 5)]
    [InlineData("Suprema", 10)]
    public void Calculate_CharacteristicWeights_Sum(string weight, int expected)
    {
        var data = BaseCreature();
        data.Characteristics.Add(new CreatureCharacteristic { Name = "C", Weight = weight });

        _sut.Calculate(data).Np.Should().Be(expected);
    }

    [Theory]
    [InlineData("Comum", 5)]
    [InlineData("Avancada", 10)]
    [InlineData("Suprema", 20)]
    public void Calculate_AbilityTiers_Sum(string tier, int expected)
    {
        var data = BaseCreature();
        data.Abilities.Add(new CreatureAbility { Name = "A", Tier = tier });

        _sut.Calculate(data).Np.Should().Be(expected);
    }

    [Theory]
    [InlineData("Comum", 1)]
    [InlineData("Incomum", 3)]
    [InlineData("Raro", 7)]
    [InlineData("Epico", 15)]
    [InlineData("Lendario", 30)]
    [InlineData("Divino", 50)]
    public void Calculate_EquipmentRarities_Sum(string rarity, int expected)
    {
        var data = BaseCreature();
        data.Equipment.Add(new CreatureEquipment { Name = "E", Rarity = rarity });

        _sut.Calculate(data).Np.Should().Be(expected);
    }

    [Fact]
    public void Calculate_UnknownWeightTierRarity_ContributeZero()
    {
        var data = BaseCreature();
        data.Characteristics.Add(new CreatureCharacteristic { Name = "C", Weight = "Bogus" });
        data.Abilities.Add(new CreatureAbility { Name = "A", Tier = "Bogus" });
        data.Equipment.Add(new CreatureEquipment { Name = "E", Rarity = "Bogus" });

        _sut.Calculate(data).Np.Should().Be(0);
    }

    [Fact]
    public void Calculate_ResolvesCategoryRange()
    {
        var data = BaseCreature();
        data.Category = "Comum";

        var result = _sut.Calculate(data);

        result.NpMin.Should().Be(40);
        result.NpMax.Should().Be(70);
    }

    [Fact]
    public void Calculate_CategoryOverflow_TrueWhenNpExceedsCeilingTimes115()
    {
        // Comum ceiling 70 → 70*1.15 = 80.5. Build NP = 81.
        var data = BaseCreature();
        data.Category = "Comum";
        data.Equipment.Add(new CreatureEquipment { Name = "E", Rarity = "Divino" });   // 50
        data.Abilities.Add(new CreatureAbility { Name = "A", Tier = "Suprema" });       // 20
        data.Characteristics.Add(new CreatureCharacteristic { Name = "C", Weight = "Suprema" }); // 10
        data.Attributes.Corpo = 2;                                                      // +1

        var result = _sut.Calculate(data);

        result.Np.Should().Be(81);
        result.CategoryOverflow.Should().BeTrue();
    }

    [Fact]
    public void Calculate_CategoryOverflow_FalseAtOrBelowCeilingTimes115()
    {
        // NP = 80, which is <= 80.5 → no overflow.
        var data = BaseCreature();
        data.Category = "Comum";
        data.Equipment.Add(new CreatureEquipment { Name = "E", Rarity = "Divino" });   // 50
        data.Abilities.Add(new CreatureAbility { Name = "A", Tier = "Suprema" });       // 20
        data.Characteristics.Add(new CreatureCharacteristic { Name = "C", Weight = "Suprema" }); // 10

        var result = _sut.Calculate(data);

        result.Np.Should().Be(80);
        result.CategoryOverflow.Should().BeFalse();
    }

    [Fact]
    public void Calculate_CategoryOverflow_FalseWhenNpWithinRange()
    {
        var data = BaseCreature();
        data.Category = "Comum";
        data.Equipment.Add(new CreatureEquipment { Name = "E", Rarity = "Divino" });   // 50 (within 40..70)

        var result = _sut.Calculate(data);

        result.Np.Should().Be(50);
        result.CategoryOverflow.Should().BeFalse();
    }

    // Documents the SEMANTIC invariant: an open-ended category (NpMax == int.MaxValue sentinel)
    // has no ceiling by definition, so it must NEVER report CategoryOverflow regardless of NP.
    // This asserts the behaviour, not a numeric guard-present-vs-absent distinction.
    [Theory]
    [InlineData("ChefeDeArco")]
    [InlineData("EntidadeSuperior")]
    public void Calculate_OpenEndedCategory_NeverReportsOverflow(string category)
    {
        var data = BaseCreature();
        data.Category = category;
        data.Attributes.Corpo = int.MaxValue;
        data.Attributes.Controle = int.MaxValue;
        data.Attributes.Vigor = int.MaxValue;

        var result = _sut.Calculate(data);

        result.NpMax.Should().Be(int.MaxValue);
        result.CategoryOverflow.Should().BeFalse();
    }

    [Fact]
    public void Calculate_CategoryOverflow_ExactlyFifteenPercentOver_IsNotOverflow()
    {
        // Fraca ceiling 40 → +15% is exactly 46. Spec is strictly "more than 15%", so 46 → false,
        // 47 → true. Guards against float drift (40*1.15 = 45.999… would wrongly flag 46).
        var data = BaseCreature();
        data.Category = "Fraca";                                                   // NpMax = 40
        data.Abilities.Add(new CreatureAbility { Name = "A1", Tier = "Suprema" });  // 20
        data.Abilities.Add(new CreatureAbility { Name = "A2", Tier = "Suprema" });  // 20
        data.Characteristics.Add(new CreatureCharacteristic { Name = "C", Weight = "Media" }); // 3
        data.Attributes.Corpo = 4;                                                  // +3

        var atBoundary = _sut.Calculate(data);
        atBoundary.Np.Should().Be(46);                 // 20+20+3+3
        atBoundary.CategoryOverflow.Should().BeFalse(); // exactly +15% → not overflow

        data.Attributes.Corpo = 5;                     // +4 → Np = 47
        var overBoundary = _sut.Calculate(data);
        overBoundary.Np.Should().Be(47);
        overBoundary.CategoryOverflow.Should().BeTrue();
    }

    [Fact]
    public void Calculate_HugeAttributes_ClampToIntRange_NoOverflow()
    {
        var data = BaseCreature();
        data.Attributes.Corpo = int.MaxValue;
        data.Attributes.Controle = int.MaxValue;
        data.Attributes.Vigor = int.MaxValue;
        data.Attributes.Presenca = int.MaxValue;
        data.Attributes.Intelecto = int.MaxValue;
        data.Attributes.Percepcao = int.MaxValue;
        data.Attributes.Vontade = int.MaxValue;
        data.Attributes.Afinidade = int.MaxValue;

        var act = () => _sut.Calculate(data);

        act.Should().NotThrow();
        _sut.Calculate(data).Np.Should().Be(int.MaxValue);
    }

    [Fact]
    public void Calculate_NullLists_DoNotThrow()
    {
        var data = new CreatureData
        {
            Attributes = new CreatureAttributes(),
            NaturalSkills = null!,
            Characteristics = null!,
            Abilities = null!,
            Equipment = null!
        };

        var act = () => _sut.Calculate(data);

        act.Should().NotThrow();
        _sut.Calculate(data).Np.Should().Be(0);
    }

    [Fact]
    public void Calculate_UnknownCategory_DoesNotThrow_AndNoOverflow()
    {
        var data = BaseCreature();
        data.Category = "NotARealCategory";
        data.Equipment.Add(new CreatureEquipment { Name = "E", Rarity = "Divino" });

        var result = _sut.Calculate(data);

        result.CategoryOverflow.Should().BeFalse();
    }
}
