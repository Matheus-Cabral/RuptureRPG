using FluentAssertions;
using Ruptura.Application.Interfaces;
using Ruptura.Application.Services;

namespace Ruptura.UnitTests.Encounters;

public class EncounterCalculatorTests
{
    private readonly IEncounterCalculator _sut = new EncounterCalculator();

    // A single neutral creature line worth `np` × `qty`, no favourable factors.
    private EncounterThreatResult Calc(
        IEnumerable<int> partyNps,
        int partySize,
        IEnumerable<(int Np, int Qty)> creatures,
        string intelligence = "Instinto",
        string terrain = "Neutro",
        string objective = "Eliminar",
        decimal pressureMult = 1.0m,
        string difficulty = "Normal",
        string duration = "Normal",
        string fceBand = "BronzeFerro")
        => _sut.Calculate(partyNps, partySize, creatures, intelligence, terrain,
            objective, pressureMult, difficulty, duration, fceBand);

    // ---- PG synergy tiers (§9.8) ----

    [Theory]
    [InlineData(1, 100)]   // ×1.0
    [InlineData(2, 110)]   // ×1.1
    [InlineData(3, 120)]   // ×1.2
    [InlineData(4, 130)]   // ×1.3
    [InlineData(5, 140)]   // ×1.4
    [InlineData(6, 150)]   // ×1.5 (cap)
    [InlineData(10, 150)]  // 6+ stays 1.5
    public void Pg_AppliesSynergyByPartySize(int partySize, int expectedPg)
    {
        var result = Calc([100], partySize, [(1, 1)]);
        result.Pg.Should().Be(expectedPg);
    }

    // ---- PE quantity multiplier band (§9.8) ----

    [Theory]
    [InlineData(1, 10)]    // base 10 × 1.0
    [InlineData(2, 25)]    // base 20 × 1.25
    [InlineData(4, 60)]    // base 40 × 1.5
    [InlineData(9, 180)]   // base 90 × 2.0
    [InlineData(21, 630)]  // base 210 × 3.0
    public void Pe_AppliesQuantityMultByTotalCount(int qty, int expectedPe)
    {
        var result = Calc([100], 1, [(10, qty)]);
        result.Pe.Should().Be(expectedPe);
    }

    // ---- PE intelligence multiplier (§9.5.3) ----

    [Theory]
    [InlineData("Instinto", 100)]
    [InlineData("Tatico", 120)]
    [InlineData("Militar", 150)]
    [InlineData("Genial", 200)]
    public void Pe_AppliesIntelligenceMult(string intelligence, int expectedPe)
    {
        var result = Calc([100], 1, [(100, 1)], intelligence: intelligence);
        result.Pe.Should().Be(expectedPe);
    }

    // ---- PE terrain multiplier ----

    [Theory]
    [InlineData("Neutro", 100)]
    [InlineData("LevementeFavoravel", 110)]
    [InlineData("Favoravel", 125)]
    [InlineData("Extremo", 150)]
    public void Pe_AppliesTerrainMult(string terrain, int expectedPe)
    {
        var result = Calc([100], 1, [(100, 1)], terrain: terrain);
        result.Pe.Should().Be(expectedPe);
    }

    // ---- PE objective multiplier ----

    [Theory]
    [InlineData("Eliminar", 100)]
    [InlineData("Sobreviver", 125)]
    [InlineData("Defender", 150)]
    [InlineData("Resgatar", 150)]
    [InlineData("MissaoCritica", 200)]
    public void Pe_AppliesObjectiveMult(string objective, int expectedPe)
    {
        var result = Calc([100], 1, [(100, 1)], objective: objective);
        result.Pe.Should().Be(expectedPe);
    }

    // ---- R label bands at boundaries ----
    // PG fixed at 100, a single neutral creature np=X qty=1 → PE=X, so R = X/100.

    [Theory]
    [InlineData(50, "MuitoFacil")]      // R=0.5   (≤0.5)
    [InlineData(85, "Facil")]           // R=0.85  (≤0.85)
    [InlineData(115, "Equilibrado")]    // R=1.15  (≤1.15)
    [InlineData(140, "Dificil")]        // R=1.4   (≤1.4)
    [InlineData(175, "MuitoDificil")]   // R=1.75  (≤1.75)
    [InlineData(200, "Extremo")]        // R=2.0   (1.75<R<3)
    [InlineData(300, "PossivelMorte")]  // R=3.0   (≥3)
    public void RLabel_ResolvesBandBoundaries(int creatureNp, string expectedLabel)
    {
        var result = Calc([100], 1, [(creatureNp, 1)]);
        result.R.Should().Be(creatureNp / 100m);
        result.RLabel.Should().Be(expectedLabel);
    }

    // ---- OA = PG × DifficultyFactor × DurationFactor (§9.9) ----

    [Theory]
    [InlineData("Seguro", "Curto", 75)]        // 100 × 0.75 × 1
    [InlineData("Normal", "Normal", 200)]      // 100 × 1.0  × 2
    [InlineData("Perigoso", "Longo", 375)]     // 100 × 1.25 × 3
    [InlineData("Apocaliptico", "Extenso", 1200)] // 100 × 3.0 × 4
    public void Oa_AppliesDifficultyAndDuration(string difficulty, string duration, int expectedOa)
    {
        var result = Calc([100], 1, [(1, 1)], difficulty: difficulty, duration: duration);
        result.Oa.Should().Be(expectedOa);
    }

    // ---- FCE band + RealStatMultiplier = 1 + (R-1) × FCE ----
    // PG=100, creature np=200 → R=2.0, so RealStatMultiplier = 1 + FCE.

    [Theory]
    [InlineData("BronzeFerro", 0.40, 1.40)]
    [InlineData("AcoPrata", 0.25, 1.25)]
    [InlineData("OuroMithril", 0.15, 1.15)]
    [InlineData("AdamanteLendario", 0.10, 1.10)]
    public void Fce_AndRealStatMultiplier(string fceBand, double expectedFce, double expectedMult)
    {
        var result = Calc([100], 1, [(200, 1)], fceBand: fceBand);
        result.R.Should().Be(2.0m);
        result.Fce.Should().Be((decimal)expectedFce);
        result.RealStatMultiplier.Should().Be((decimal)expectedMult);
    }

    // ---- Pressão applied vs not ----

    [Fact]
    public void Pressure_NotApplied_LeavesPeUnchanged()
    {
        var result = Calc([100], 1, [(100, 1)], pressureMult: 1.0m);
        result.Pe.Should().Be(100);
        result.R.Should().Be(1.0m);
    }

    [Fact]
    public void Pressure_Applied_MultipliesPe()
    {
        var result = Calc([100], 1, [(100, 1)], pressureMult: 1.5m);
        result.Pe.Should().Be(150);
        result.R.Should().Be(1.5m);
    }

    // ---- Empty party → PG=0, no divide-by-zero ----

    [Fact]
    public void EmptyParty_YieldsSafeResult_NoDivideByZero()
    {
        var act = () => Calc([], 0, [(100, 1)]);
        act.Should().NotThrow();

        var result = Calc([], 0, [(100, 1)]);
        result.Pg.Should().Be(0);
        result.R.Should().Be(0m);
        result.RLabel.Should().Be("MuitoFacil");   // documented sentinel: RLabelFor(0)
        result.Oa.Should().Be(0);
        // RealStatMultiplier = 1 + (0-1) × 0.40 = 0.60
        result.RealStatMultiplier.Should().Be(0.60m);
    }

    // ---- Unknown map keys → neutral (1.0 factor; FCE → 0) ----

    [Fact]
    public void UnknownKeys_TreatedAsNeutral()
    {
        var result = Calc([100], 1, [(100, 1)],
            intelligence: "???", terrain: "???", objective: "???",
            difficulty: "???", duration: "???", fceBand: "???");

        result.Pe.Should().Be(100);          // all PE mults neutral → 1.0
        result.R.Should().Be(1.0m);
        result.Oa.Should().Be(100);          // difficulty 1.0 × duration 1
        result.Fce.Should().Be(0m);          // unknown FCE band → 0
        result.RealStatMultiplier.Should().Be(1.0m); // 1 + (R-1)*0
    }

    // ---- §9.8 vignette: 5 weak goblins vs a typical party → "MuitoFacil" ----

    [Fact]
    public void Vignette_FiveWeakGoblins_IsMuitoFacil()
    {
        // Party: 4 characters, NP 30 each. PG = 120 × Synergy(4)=1.3 = 156.
        // Creatures: 5 goblins, NP 4 each. base=20, count=5 → QuantityMult 1.5 → PE=30.
        // R = 30 / 156 ≈ 0.192 → MuitoFacil.
        var result = Calc([30, 30, 30, 30], 4, [(4, 5)]);

        result.Pg.Should().Be(156);
        result.Pe.Should().Be(30);
        result.R.Should().BeLessThanOrEqualTo(0.5m);
        result.RLabel.Should().Be("MuitoFacil");
    }
}
