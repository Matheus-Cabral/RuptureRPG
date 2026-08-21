using System.Text.Json;
using FluentAssertions;
using Ruptura.Application.Services;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.UnitTests.Application;

public class CharacterStatsCalculatorTests
{
    private readonly CharacterStatsCalculator _sut = new();

    private static CatalogEntry Skill(Guid id, string relatedAttribute) =>
        SkillNamed(id, "Test Skill", relatedAttribute);

    private static CatalogEntry SkillNamed(Guid id, string name, string relatedAttribute) => new()
    {
        Id = id, Type = CatalogEntryType.Skill, Name = name,
        DataJson = JsonSerializer.Serialize(new { Area = "Combate — Armas", RelatedAttribute = relatedAttribute })
    };

    private static CatalogEntry Talent(Guid id, string powerTier) => new()
    {
        Id = id, Type = CatalogEntryType.Talent, Name = "Test Talent",
        DataJson = JsonSerializer.Serialize(new { Category = "Combate", Effect = "x", PowerTier = powerTier })
    };

    private static CatalogEntry Spell(Guid id, string powerTier) => new()
    {
        Id = id, Type = CatalogEntryType.Spell, Name = "Test Spell",
        DataJson = JsonSerializer.Serialize(new
        {
            School = "Evocação", ComplexityPaCost = "Menor (1 PA)", Range = "Curta", Area = "Único Alvo",
            Duration = "Instantânea", Test = "Absoluto", Damage = "1d6", Effect = "x", PowerTier = powerTier
        })
    };

    private static CatalogEntry Technique(Guid id, string powerTier) => new()
    {
        Id = id, Type = CatalogEntryType.Technique, Name = "Test Technique",
        DataJson = JsonSerializer.Serialize(new
        {
            Style = "Espadas", Category = "Técnica", PaCost = "1", Damage = "", Effect = "x", PowerTier = powerTier
        })
    };

    private static CatalogEntry Equipment(
        Guid id, string category, string rarity, int attackBonus = 0, int damageBonus = 0,
        int defenseBonus = 0, string? diceCategory = null, int? armorReduction = null, decimal weight = 0) => new()
    {
        Id = id, Type = CatalogEntryType.EquipmentItem, Name = "Test Item",
        DataJson = JsonSerializer.Serialize(new
        {
            Category = category, Rarity = rarity, AttackBonus = attackBonus, DamageBonus = damageBonus,
            DefenseBonus = defenseBonus, WeaponDiceCategory = diceCategory,
            ArmorDamageReduction = armorReduction, Weight = weight
        })
    };

    // ── Attribute modifier / grade bonus ────────────────────────────────────

    [Theory]
    [InlineData(1, -1, 0)]
    [InlineData(2, 0, 1)]
    [InlineData(5, 3, 4)]
    public void Calculate_AttributeModifierAndGradeBonus_MatchGdd(int score, int expectedModifier, int expectedGrade)
    {
        var data = new CharacterSheetData { Attributes = new CharacterAttributes { Corpo = score } };

        var result = _sut.Calculate(data, new Dictionary<Guid, CatalogEntry>());

        result.AttributeModifiers["Corpo"].Should().Be(expectedModifier);
        result.AttributeGradeBonuses["Corpo"].Should().Be(expectedGrade);
    }

    // ── Skill grade bonus thresholds ────────────────────────────────────────

    [Theory]
    [InlineData(0, -2)]
    [InlineData(9, -2)]
    [InlineData(10, 0)]
    [InlineData(24, 0)]
    [InlineData(25, 1)]
    [InlineData(49, 1)]
    [InlineData(50, 2)]
    [InlineData(74, 2)]
    [InlineData(75, 3)]
    [InlineData(99, 3)]
    [InlineData(100, 4)]
    [InlineData(250, 4)]
    public void Calculate_SkillGradeBonus_MatchesGdrThresholdTable(int points, int expectedGrade)
    {
        var skillId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Skills = [new CharacterSkillEntry { CatalogEntryId = skillId, Points = points }]
        };
        var catalog = new Dictionary<Guid, CatalogEntry> { [skillId] = Skill(skillId, "Controle") };

        var result = _sut.Calculate(data, catalog);

        result.SkillGradeBonuses[skillId].Should().Be(expectedGrade);
    }

    // ── PV Máximo (per Ranking) ──────────────────────────────────────────────

    [Theory]
    [InlineData("Bronze", 0)]
    [InlineData("Ferro", 5)]
    [InlineData("Aço", 10)]
    [InlineData("Prata", 15)]
    [InlineData("Ouro", 20)]
    [InlineData("Mithril", 25)]
    [InlineData("Adamante", 30)]
    [InlineData("Lendário", 35)]
    public void Calculate_MaxHp_Is10PlusVigorTimes2PlusRankingBonus(string ranking, int expectedRankingBonus)
    {
        var data = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Vigor = 3 },
            GuildRegistry = new CharacterGuildRegistry { Ranking = ranking }
        };

        var result = _sut.Calculate(data, new Dictionary<Guid, CatalogEntry>());

        result.MaxHp.Should().Be(10 + 3 * 2 + expectedRankingBonus);
    }

    // ── Movement / Initiative ────────────────────────────────────────────────

    [Fact]
    public void Calculate_Movement_Is4PlusControleModifier()
    {
        var data = new CharacterSheetData { Attributes = new CharacterAttributes { Controle = 4 } };

        var result = _sut.Calculate(data, new Dictionary<Guid, CatalogEntry>());

        result.Movement.Should().Be(4 + (4 - 2));
    }

    [Fact]
    public void Calculate_Initiative_IsControleModifier()
    {
        var data = new CharacterSheetData { Attributes = new CharacterAttributes { Controle = 5 } };

        var result = _sut.Calculate(data, new Dictionary<Guid, CatalogEntry>());

        result.Initiative.Should().Be(5 - 2);
    }

    // ── Passive Defense + Damage Reduction from equipped armor/shield ───────

    [Fact]
    public void Calculate_PassiveDefenseAndDamageReduction_OnlyCountEquippedArmorAndShield()
    {
        var armorId = Guid.NewGuid();
        var shieldId = Guid.NewGuid();
        var unequippedArmorId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Controle = 3 },
            Equipment =
            [
                new CharacterEquipmentEntry { CatalogEntryId = armorId, IsEquipped = true },
                new CharacterEquipmentEntry { CatalogEntryId = shieldId, IsEquipped = true },
                new CharacterEquipmentEntry { CatalogEntryId = unequippedArmorId, IsEquipped = false }
            ]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [armorId] = Equipment(armorId, "armadura", "Comum", defenseBonus: 2, armorReduction: 2),
            [shieldId] = Equipment(shieldId, "escudo", "Comum", defenseBonus: 1),
            [unequippedArmorId] = Equipment(unequippedArmorId, "armadura", "Comum", defenseBonus: 99, armorReduction: 99)
        };

        var result = _sut.Calculate(data, catalog);

        result.PassiveDefense.Should().Be(10 + (3 - 2) + 2 + 1);
        result.DamageReduction.Should().Be(2);
    }

    // ── Category matching is case/whitespace-insensitive (free-text catalog field) ──

    [Theory]
    [InlineData("Armadura")]
    [InlineData("ARMADURA")]
    [InlineData(" armadura ")]
    public void Calculate_ArmorCategory_MatchesRegardlessOfCaseOrWhitespace(string category)
    {
        var armorId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Equipment = [new CharacterEquipmentEntry { CatalogEntryId = armorId, IsEquipped = true }]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [armorId] = Equipment(armorId, category, "Comum", defenseBonus: 2, armorReduction: 3)
        };

        var result = _sut.Calculate(data, catalog);

        result.PassiveDefense.Should().Be(10 + -1 + 2); // Controle defaults to 1 → modifier -1
        result.DamageReduction.Should().Be(3);
    }

    [Theory]
    [InlineData("Arma")]
    [InlineData("ARMA")]
    [InlineData(" arma ")]
    public void Calculate_WeaponCategory_MatchesRegardlessOfCaseOrWhitespace(string category)
    {
        var weaponId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Equipment = [new CharacterEquipmentEntry { CatalogEntryId = weaponId, IsEquipped = true }]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [weaponId] = Equipment(weaponId, category, "Comum", diceCategory: "Leve")
        };

        var result = _sut.Calculate(data, catalog);

        result.Weapons.Should().ContainSingle();
    }

    // ── Active Defense: Esquiva (Controle) / Bloqueio (Vigor) — GDD §7.4.1 ──

    [Fact]
    public void Calculate_ActiveDefense_UninvestedEsquivaAndBloqueio_UseAttributeGradePlusUntrainedSkillGrade()
    {
        var data = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Controle = 4, Vigor = 3 } // grade bonuses +3, +2
        };

        var result = _sut.Calculate(data, new Dictionary<Guid, CatalogEntry>());

        result.EsquivaBonus.Should().Be(3 + -2); // Controle grade bonus + Sem Treinamento
        result.BloqueioBonus.Should().Be(2 + -2); // Vigor grade bonus + Sem Treinamento
    }

    [Fact]
    public void Calculate_ActiveDefense_InvestedEsquivaAndBloqueio_AddTheirSkillGradeBonus()
    {
        var esquivaId = Guid.NewGuid();
        var bloqueioId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Controle = 4, Vigor = 3 }, // grade bonuses +3, +2
            Skills =
            [
                new CharacterSkillEntry { CatalogEntryId = esquivaId, Points = 50 },  // grade +2
                new CharacterSkillEntry { CatalogEntryId = bloqueioId, Points = 75 }  // grade +3
            ]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [esquivaId] = SkillNamed(esquivaId, "Esquiva", "Controle"),
            [bloqueioId] = SkillNamed(bloqueioId, "Bloqueio", "Vigor")
        };

        var result = _sut.Calculate(data, catalog);

        result.EsquivaBonus.Should().Be(3 + 2);
        result.BloqueioBonus.Should().Be(2 + 3);
    }

    // ── Carry capacity / current weight ─────────────────────────────────────

    [Fact]
    public void Calculate_CarryCapacity_IsCorpoScoreTimes5_AndWeightSumsQuantityTimesItemWeight()
    {
        var itemId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Corpo = 4 },
            Equipment = [new CharacterEquipmentEntry { CatalogEntryId = itemId, Quantity = 3 }]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [itemId] = Equipment(itemId, "item", "Comum", weight: 1.5m)
        };

        var result = _sut.Calculate(data, catalog);

        result.CarryCapacity.Should().Be(4 * 5);
        result.CurrentWeight.Should().Be(4.5m);
    }

    // ── Weapon attack/damage row ─────────────────────────────────────────────

    [Fact]
    public void Calculate_EquippedWeaponWithLinkedSkill_ProducesAttackBonusAndDamageFormula()
    {
        var skillId = Guid.NewGuid();
        var weaponId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Controle = 4 }, // modifier +2, grade bonus +3
            Skills = [new CharacterSkillEntry { CatalogEntryId = skillId, Points = 30 }], // grade bonus +1
            Equipment =
            [
                new CharacterEquipmentEntry
                {
                    CatalogEntryId = weaponId, IsEquipped = true, LinkedSkillEntryId = skillId
                }
            ]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [skillId] = Skill(skillId, "Controle"),
            [weaponId] = Equipment(weaponId, "arma", "Comum", damageBonus: 2, diceCategory: "Média")
        };

        var result = _sut.Calculate(data, catalog);

        var row = result.Weapons.Should().ContainSingle().Subject;
        row.CatalogEntryId.Should().Be(weaponId);
        row.AttackBonus.Should().Be(3 + 1); // attribute grade bonus + skill grade bonus
        row.DamageFormula.Should().Be("1d8 +5"); // dice(Média) + (attr modifier +2 + skill grade +1 + item damageBonus +2)
    }

    [Fact]
    public void Calculate_UnequippedWeapon_DoesNotAppearInWeaponsTable()
    {
        var weaponId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Equipment = [new CharacterEquipmentEntry { CatalogEntryId = weaponId, IsEquipped = false }]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [weaponId] = Equipment(weaponId, "arma", "Comum", diceCategory: "Leve")
        };

        var result = _sut.Calculate(data, catalog);

        result.Weapons.Should().BeEmpty();
    }

    // ── NP ────────────────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_Np_SumsAttributeAndSkillGradeBonusesPlusTalentAndEquipmentWeights()
    {
        var talentId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var skillId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            // 8 attributes at score 2 → grade bonus 1 each → 8 total
            Attributes = new CharacterAttributes
            {
                Corpo = 2, Controle = 2, Vigor = 2, Presenca = 2,
                Intelecto = 2, Percepcao = 2, Vontade = 2, Afinidade = 2
            },
            Skills = [new CharacterSkillEntry { CatalogEntryId = skillId, Points = 25 }], // grade +1
            Talents = [new CharacterCatalogRefEntry { CatalogEntryId = talentId }],       // "maior" → 5
            Equipment = [new CharacterEquipmentEntry { CatalogEntryId = itemId, Quantity = 1 }] // "Raro" → 7
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [skillId] = Skill(skillId, "Controle"),
            [talentId] = Talent(talentId, "maior"),
            [itemId] = Equipment(itemId, "item", "Raro")
        };

        var result = _sut.Calculate(data, catalog);

        result.Np.Should().Be(8 + 1 + 5 + 7);
    }

    [Fact]
    public void Calculate_Np_IncludesSpellAndTechniquePowerTierWeights()
    {
        var spellId = Guid.NewGuid();
        var techniqueId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Spells = [new CharacterCatalogRefEntry { CatalogEntryId = spellId }],         // "avançada" → 10
            Techniques = [new CharacterCatalogRefEntry { CatalogEntryId = techniqueId }]  // "suprema" → 20
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [spellId] = Spell(spellId, "avançada"),
            [techniqueId] = Technique(techniqueId, "suprema")
        };

        var result = _sut.Calculate(data, catalog);

        // Base attributes default to score 1 → grade bonus 0 each, so NP is isolated to
        // the Poder de Especialização (Habilidades) contribution — GDD §6.8.
        result.Np.Should().Be(10 + 20);
    }

    [Fact]
    public void Calculate_Np_SpellPowerTierLookup_IsCaseAndWhitespaceInsensitive()
    {
        // Mirrors CharacterStatsCalculator.CategoryIs: a GM typing "Avançada" (capitalized,
        // matching every other catalog value's convention) instead of the seeded lowercase
        // "avançada" must not silently drop the spell's NP contribution to 0.
        var spellId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Spells = [new CharacterCatalogRefEntry { CatalogEntryId = spellId }]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [spellId] = Spell(spellId, " Avançada ")
        };

        var result = _sut.Calculate(data, catalog);

        result.Np.Should().Be(10);
    }

    // ── Finding 2: untrained-skill grade drift ──────────────────────────────

    [Fact]
    public void Calculate_WeaponLinkedToUninvestedSkill_UsesUntrainedGradeBonus()
    {
        var skillId = Guid.NewGuid();
        var weaponId = Guid.NewGuid();
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [skillId] = Skill(skillId, "Controle"),
            [weaponId] = Equipment(weaponId, "arma", "Comum", diceCategory: "Leve")
        };

        // Case A: weapon links to a skill the character never invested in — not present in
        // Skills[] at all (e.g. dangling link left after the skill was removed).
        var dataNotInvested = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Controle = 4 }, // grade bonus +3
            Equipment =
            [
                new CharacterEquipmentEntry
                {
                    CatalogEntryId = weaponId, IsEquipped = true, LinkedSkillEntryId = skillId
                }
            ]
        };

        // Case B: same skill, explicitly invested with 0 points — GDD's "Sem Treinamento" (−2).
        var dataZeroPoints = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Controle = 4 },
            Skills = [new CharacterSkillEntry { CatalogEntryId = skillId, Points = 0 }],
            Equipment =
            [
                new CharacterEquipmentEntry
                {
                    CatalogEntryId = weaponId, IsEquipped = true, LinkedSkillEntryId = skillId
                }
            ]
        };

        var resultNotInvested = _sut.Calculate(dataNotInvested, catalog);
        var resultZeroPoints = _sut.Calculate(dataZeroPoints, catalog);

        var rowNotInvested = resultNotInvested.Weapons.Should().ContainSingle().Subject;
        var rowZeroPoints = resultZeroPoints.Weapons.Should().ContainSingle().Subject;

        // Before the fix, GetValueOrDefault on the (empty for Case A) skill-grade dictionary
        // silently returned 0 ("Básico") instead of the correct −2 ("Sem Treinamento").
        rowNotInvested.AttackBonus.Should().Be(3 + -2); // attribute grade bonus (3) + untrained skill grade (−2)
        rowNotInvested.AttackBonus.Should().Be(rowZeroPoints.AttackBonus);
    }

    // ── Finding 1a: malformed catalog DataJson must not 500 the read path ───

    [Fact]
    public void Calculate_MalformedCatalogDataJson_DoesNotThrow_TreatsEntryAsAbsent()
    {
        var itemId = Guid.NewGuid();
        var talentId = Guid.NewGuid();
        var skillId = Guid.NewGuid();
        var weaponId = Guid.NewGuid();

        var malformedEquipment = new CatalogEntry
        {
            Id = itemId, Type = CatalogEntryType.EquipmentItem, Name = "Broken Item",
            DataJson = """{"Weight":"2kg"}""" // Weight is decimal — a string value fails to deserialize
        };
        var malformedTalent = new CatalogEntry
        {
            Id = talentId, Type = CatalogEntryType.Talent, Name = "Broken Talent",
            DataJson = """{"PowerTier":123}""" // PowerTier is string — a number value fails to deserialize
        };
        var malformedSkill = new CatalogEntry
        {
            Id = skillId, Type = CatalogEntryType.Skill, Name = "Broken Skill",
            DataJson = "not json at all"
        };
        var validWeapon = Equipment(weaponId, "arma", "Comum", diceCategory: "Leve");

        var data = new CharacterSheetData
        {
            Talents = [new CharacterCatalogRefEntry { CatalogEntryId = talentId }],
            Equipment =
            [
                new CharacterEquipmentEntry { CatalogEntryId = itemId, Quantity = 2 },
                new CharacterEquipmentEntry
                {
                    CatalogEntryId = weaponId, IsEquipped = true, LinkedSkillEntryId = skillId
                }
            ]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [itemId] = malformedEquipment,
            [talentId] = malformedTalent,
            [skillId] = malformedSkill,
            [weaponId] = validWeapon
        };

        CharacterDerivedStats? result = null;
        var act = () => result = _sut.Calculate(data, catalog);

        act.Should().NotThrow();
        result.Should().NotBeNull();
        result!.CurrentWeight.Should().Be(0); // malformed item's Weight couldn't be read → treated as absent
        var row = result.Weapons.Should().ContainSingle().Subject; // weapon itself deserializes fine
        row.AttackBonus.Should().Be(0); // its linked skill's malformed data is treated as no linked skill
    }
}
