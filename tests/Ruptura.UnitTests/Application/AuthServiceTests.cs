using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Infrastructure.Identity;
using Ruptura.Infrastructure.Services;
using Ruptura.Infrastructure.Settings;
using Ruptura.Shared.Auth;
using Microsoft.Extensions.Options;

namespace Ruptura.UnitTests.Application;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<JwtService> _jwtServiceMock;
    private readonly Mock<IInviteCodeRepository> _inviteRepoMock;
    private readonly AuthService _sut;

    private static readonly Faker Faker = new();

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var jwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "super-secret-key-for-testing-purposes-only!!",
            Issuer = "RupturaAPI",
            Audience = "RupturaClient",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        });
        _jwtServiceMock = new Mock<JwtService>(jwtSettings) { CallBase = true };
        _inviteRepoMock = new Mock<IInviteCodeRepository>();

        _sut = new AuthService(
            _userManagerMock.Object,
            _jwtServiceMock.Object,
            _inviteRepoMock.Object,
            jwtSettings);
    }

    // ── Login ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessWithTokens()
    {
        var user = BuildUser(UserRole.Player);
        _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "ValidPass1")).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.LoginAsync(new LoginRequest
        {
            Email = user.Email!,
            Password = "ValidPass1"
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ReturnsFailure()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.LoginAsync(new LoginRequest
        {
            Email = "ghost@example.com",
            Password = "anything"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Auth.InvalidCredentials);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
    {
        var user = BuildUser(UserRole.Player);
        _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(false);

        var result = await _sut.LoginAsync(new LoginRequest
        {
            Email = user.Email!,
            Password = "WrongPass"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Auth.InvalidCredentials);
    }

    // ── Register GM ──────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterGameMasterAsync_WithValidData_CreatesUserAndReturnsTokens()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.RegisterGameMasterAsync(new RegisterRequest
        {
            DisplayName = "Dungeon Master",
            Email = "dm@example.com",
            Password = "ValidPass1",
            ConfirmPassword = "ValidPass1"
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.User.Role.Should().Be(UserRole.GameMaster.ToString());
    }

    [Fact]
    public async Task RegisterGameMasterAsync_WithExistingEmail_ReturnsFailure()
    {
        var existing = BuildUser(UserRole.GameMaster);
        _userManagerMock.Setup(m => m.FindByEmailAsync(existing.Email!)).ReturnsAsync(existing);

        var result = await _sut.RegisterGameMasterAsync(new RegisterRequest
        {
            DisplayName = "Another DM",
            Email = existing.Email!,
            Password = "ValidPass1",
            ConfirmPassword = "ValidPass1"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Auth.EmailAlreadyExists);
    }

    // ── Register Player ───────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterPlayerAsync_WithValidInviteCode_CreatesPlayerUser()
    {
        var invite = new InviteCode
        {
            Code = "VALID123",
            CreatedByGameMasterId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        _inviteRepoMock.Setup(r => r.GetByCodeAsync("VALID123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        _inviteRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.RegisterPlayerAsync(new RegisterPlayerRequest
        {
            DisplayName = "Brave Hero",
            Email = "hero@example.com",
            Password = "ValidPass1",
            ConfirmPassword = "ValidPass1",
            InviteCode = "VALID123"
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.User.Role.Should().Be(UserRole.Player.ToString());
    }

    [Fact]
    public async Task RegisterPlayerAsync_WithValidInviteCode_SetsRecruitingGameMaster()
    {
        var gmId = Guid.NewGuid();
        var invite = new InviteCode
        {
            Code = "VALID123",
            CreatedByGameMasterId = gmId,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        _inviteRepoMock.Setup(r => r.GetByCodeAsync("VALID123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        ApplicationUser? createdUser = null;
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((u, _) => createdUser = u)
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        _inviteRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.RegisterPlayerAsync(new RegisterPlayerRequest
        {
            DisplayName = "Brave Hero",
            Email = "hero2@example.com",
            Password = "ValidPass1",
            ConfirmPassword = "ValidPass1",
            InviteCode = "VALID123"
        });

        result.IsSuccess.Should().BeTrue();
        createdUser.Should().NotBeNull();
        createdUser!.RecruitedByGameMasterId.Should().Be(gmId);
    }

    [Fact]
    public async Task RegisterPlayerAsync_WithInvalidInviteCode_ReturnsFailure()
    {
        _inviteRepoMock.Setup(r => r.GetByCodeAsync("INVALID", It.IsAny<CancellationToken>()))
            .ReturnsAsync((InviteCode?)null);

        var result = await _sut.RegisterPlayerAsync(new RegisterPlayerRequest
        {
            DisplayName = "Player",
            Email = "player@example.com",
            Password = "ValidPass1",
            ConfirmPassword = "ValidPass1",
            InviteCode = "INVALID"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Auth.InvalidInviteCode);
    }

    [Fact]
    public async Task RegisterPlayerAsync_WithExpiredInviteCode_ReturnsFailure()
    {
        var expired = new InviteCode
        {
            Code = "EXPIRED",
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };

        _inviteRepoMock.Setup(r => r.GetByCodeAsync("EXPIRED", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expired);

        var result = await _sut.RegisterPlayerAsync(new RegisterPlayerRequest
        {
            DisplayName = "Late Player",
            Email = "late@example.com",
            Password = "ValidPass1",
            ConfirmPassword = "ValidPass1",
            InviteCode = "EXPIRED"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Auth.InvalidInviteCode);
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
    {
        var user = BuildUser(UserRole.Player);
        user.RefreshToken = "valid-refresh-token";
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(1);

        _userManagerMock.Setup(m => m.Users)
            .Returns(new[] { user }.AsQueryable());
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = "valid-refresh-token"
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.RefreshToken.Should().NotBe("valid-refresh-token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ReturnsFailure()
    {
        var user = BuildUser(UserRole.Player);
        user.RefreshToken = "old-token";
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(-1);

        _userManagerMock.Setup(m => m.Users)
            .Returns(new[] { user }.AsQueryable());

        var result = await _sut.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = "old-token"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Auth.InvalidRefreshToken);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ApplicationUser BuildUser(UserRole role) => new()
    {
        Id = Guid.NewGuid(),
        Email = Faker.Internet.Email(),
        UserName = Faker.Internet.UserName(),
        DisplayName = Faker.Name.FullName(),
        Role = role
    };
}
