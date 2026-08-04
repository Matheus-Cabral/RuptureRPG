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

namespace Ruptura.UnitTests.Application;

public class InviteCodeServiceTests
{
    private readonly Mock<IInviteCodeRepository> _repoMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly InviteCodeService _sut;

    private static readonly Faker Faker = new();

    public InviteCodeServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _sut = new InviteCodeService(_repoMock.Object, _userManagerMock.Object);
    }

    [Fact]
    public async Task GenerateAsync_CreatesUniqueCodeAndPersists()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<InviteCode>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var gameMasterId = Guid.NewGuid();
        var result = await _sut.GenerateAsync(gameMasterId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Code.Should().NotBeNullOrEmpty();
        result.Value.IsUsed.Should().BeFalse();
        result.Value.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        _repoMock.Verify(r => r.AddAsync(
            It.Is<InviteCode>(c =>
                c.CreatedByGameMasterId == gameMasterId &&
                c.Code.Length > 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByCodeAsync_WhenCodeExists_ReturnsCode()
    {
        var invite = new InviteCode
        {
            Id = Guid.NewGuid(),
            Code = "ABC123",
            CreatedByGameMasterId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        _repoMock.Setup(r => r.GetByCodeAsync("ABC123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);

        var result = await _sut.GetByCodeAsync("ABC123");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Code.Should().Be("ABC123");
    }

    [Fact]
    public async Task GetByCodeAsync_WhenCodeDoesNotExist_ReturnsFailure()
    {
        _repoMock.Setup(r => r.GetByCodeAsync("NOTFOUND", It.IsAny<CancellationToken>()))
            .ReturnsAsync((InviteCode?)null);

        var result = await _sut.GetByCodeAsync("NOTFOUND");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Invite.NotFound);
    }

    [Fact]
    public async Task GetByGameMasterAsync_ReturnsOnlyCodesForThatGM()
    {
        var gmId = Guid.NewGuid();
        var codes = new List<InviteCode>
        {
            new() { Id = Guid.NewGuid(), Code = "A", CreatedByGameMasterId = gmId, ExpiresAt = DateTime.UtcNow.AddDays(1) },
            new() { Id = Guid.NewGuid(), Code = "B", CreatedByGameMasterId = gmId, ExpiresAt = DateTime.UtcNow.AddDays(1) }
        };

        _repoMock.Setup(r => r.GetByGameMasterAsync(gmId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(codes);

        var result = await _sut.GetByGameMasterAsync(gmId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByGameMasterAsync_WhenCodeIsUsed_PopulatesRedeemerInfo()
    {
        var gmId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var usedAt = DateTime.UtcNow.AddHours(-1);
        var codes = new List<InviteCode>
        {
            new()
            {
                Id = Guid.NewGuid(), Code = "USED1", CreatedByGameMasterId = gmId,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                UsedByPlayerId = playerId, UsedAt = usedAt
            }
        };
        var player = new ApplicationUser
        {
            Id = playerId, DisplayName = "Brave Hero", Email = "hero@example.com", Role = UserRole.Player
        };

        _repoMock.Setup(r => r.GetByGameMasterAsync(gmId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(codes);
        _userManagerMock.Setup(m => m.FindByIdAsync(playerId.ToString())).ReturnsAsync(player);

        var result = await _sut.GetByGameMasterAsync(gmId);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value!.Single();
        response.UsedAt.Should().Be(usedAt);
        response.RedeemedByDisplayName.Should().Be("Brave Hero");
        response.RedeemedByEmail.Should().Be("hero@example.com");
    }

    [Fact]
    public async Task GetByGameMasterAsync_WhenCodeIsUnused_LeavesRedeemerInfoNull()
    {
        var gmId = Guid.NewGuid();
        var codes = new List<InviteCode>
        {
            new() { Id = Guid.NewGuid(), Code = "UNUSED", CreatedByGameMasterId = gmId, ExpiresAt = DateTime.UtcNow.AddDays(1) }
        };

        _repoMock.Setup(r => r.GetByGameMasterAsync(gmId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(codes);

        var result = await _sut.GetByGameMasterAsync(gmId);

        var response = result.Value!.Single();
        response.UsedAt.Should().BeNull();
        response.RedeemedByDisplayName.Should().BeNull();
        response.RedeemedByEmail.Should().BeNull();
        _userManagerMock.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetByCodeAsync_WhenCodeIsUsed_DoesNotExposeRedeemerInfo()
    {
        // GetByCodeAsync backs the [AllowAnonymous] validate endpoint — it must never
        // leak the redeeming player's name/email to an unauthenticated caller.
        var invite = new InviteCode
        {
            Id = Guid.NewGuid(),
            Code = "ABC123",
            CreatedByGameMasterId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            UsedByPlayerId = Guid.NewGuid(),
            UsedAt = DateTime.UtcNow.AddHours(-1)
        };

        _repoMock.Setup(r => r.GetByCodeAsync("ABC123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);

        var result = await _sut.GetByCodeAsync("ABC123");

        result.Value!.RedeemedByDisplayName.Should().BeNull();
        result.Value.RedeemedByEmail.Should().BeNull();
        _userManagerMock.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }
}
