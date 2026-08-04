using Bogus;
using FluentAssertions;
using Moq;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Services;

namespace Ruptura.UnitTests.Application;

public class InviteCodeServiceTests
{
    private readonly Mock<IInviteCodeRepository> _repoMock = new();
    private readonly InviteCodeService _sut;

    private static readonly Faker Faker = new();

    public InviteCodeServiceTests()
    {
        _sut = new InviteCodeService(_repoMock.Object);
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
}
