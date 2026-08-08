using FluentAssertions;
using Ruptura.Shared.Campaigns;
using Xunit;

namespace Ruptura.UnitTests.Campaigns;

public class DungeonPressureTests
{
    [Theory]
    [InlineData(0, "Estavel", 1.00)]
    [InlineData(24, "Estavel", 1.00)]
    [InlineData(25, "Agravado", 1.10)]
    [InlineData(59, "Agravado", 1.10)]
    [InlineData(60, "Critico", 1.25)]
    [InlineData(89, "Critico", 1.25)]
    [InlineData(90, "Colapso", 1.50)]
    [InlineData(100, "Colapso", 1.50)]
    public void StateFor_MapsRangeToStateAndMultiplier(int pressure, string key, decimal mult)
    {
        var (stateKey, peMultiplier) = DungeonPressure.StateFor(pressure);
        stateKey.Should().Be(key);
        peMultiplier.Should().Be(mult);
    }
}
