using FluentAssertions;
using Ruptura.Application.Validators.Auth;
using Ruptura.Shared.Auth;

namespace Ruptura.UnitTests.Application;

public class ChangePasswordRequestValidatorTests
{
    private readonly ChangePasswordRequestValidator _sut = new();

    [Fact]
    public void WithStrongMatchingPasswords_Succeeds()
    {
        var result = _sut.Validate(new ChangePasswordRequest
        {
            NewPassword = "NewPass123",
            ConfirmNewPassword = "NewPass123"
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void WhenConfirmationDiffers_Fails()
    {
        var result = _sut.Validate(new ChangePasswordRequest
        {
            NewPassword = "NewPass123",
            ConfirmNewPassword = "Different123"
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordRequest.ConfirmNewPassword));
    }

    [Theory]
    [InlineData("")]                // empty
    [InlineData("Ab1")]             // too short
    [InlineData("alllowercase1")]   // no uppercase
    [InlineData("ALLUPPERCASE1")]   // no lowercase
    [InlineData("NoDigitsHere")]    // no digit
    public void WithPasswordBreakingThePolicy_Fails(string password)
    {
        var result = _sut.Validate(new ChangePasswordRequest
        {
            NewPassword = password,
            ConfirmNewPassword = password
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordRequest.NewPassword));
    }
}
