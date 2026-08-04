using FluentAssertions;
using Ruptura.Domain.Entities;

namespace Ruptura.UnitTests.Domain;

public class InviteCodeTests
{
    [Fact]
    public void IsValid_WhenNotUsedAndNotExpired_ReturnsTrue()
    {
        var code = new InviteCode
        {
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        code.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenAlreadyUsed_ReturnsFalse()
    {
        var code = new InviteCode
        {
            UsedByPlayerId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        code.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenExpired_ReturnsFalse()
    {
        var code = new InviteCode
        {
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };

        code.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenUsedAndExpired_ReturnsFalse()
    {
        var code = new InviteCode
        {
            UsedByPlayerId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };

        code.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsUsed_WhenUsedByPlayerIdIsNull_ReturnsFalse()
    {
        var code = new InviteCode { UsedByPlayerId = null };
        code.IsUsed.Should().BeFalse();
    }

    [Fact]
    public void IsUsed_WhenUsedByPlayerIdIsSet_ReturnsTrue()
    {
        var code = new InviteCode { UsedByPlayerId = Guid.NewGuid() };
        code.IsUsed.Should().BeTrue();
    }
}
