using FluentAssertions;
using Ruptura.Application.Validators.Campaigns;
using Ruptura.Shared.Campaigns;

namespace Ruptura.UnitTests.Application;

public class RemoveMemberRequestValidatorTests
{
    private readonly RemoveMemberRequestValidator _sut = new();

    [Fact]
    public void WithPassword_Succeeds() =>
        _sut.Validate(new RemoveMemberRequest { Password = "GmPass123" }).IsValid.Should().BeTrue();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void WithoutPassword_Fails(string password) =>
        _sut.Validate(new RemoveMemberRequest { Password = password }).IsValid.Should().BeFalse();
}
