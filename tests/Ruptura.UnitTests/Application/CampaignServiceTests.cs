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
using Ruptura.Shared.Campaigns;

namespace Ruptura.UnitTests.Application;

public class CampaignServiceTests
{
    private readonly Mock<ICampaignRepository> _campaignRepoMock = new();
    private readonly Mock<ICampaignMembershipRepository> _membershipRepoMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly CampaignService _sut;

    private static readonly Faker Faker = new();

    public CampaignServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _sut = new CampaignService(
            _campaignRepoMock.Object,
            _membershipRepoMock.Object,
            _userManagerMock.Object);
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_PersistsCampaignOwnedByGameMaster()
    {
        _campaignRepoMock.Setup(r => r.AddAsync(It.IsAny<Campaign>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _campaignRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var gmId = Guid.NewGuid();
        var result = await _sut.CreateAsync(gmId, new CreateCampaignRequest { Name = "The Sunken Gate" });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("The Sunken Gate");

        _campaignRepoMock.Verify(r => r.AddAsync(
            It.Is<Campaign>(c => c.GameMasterId == gmId && c.Name == "The Sunken Gate"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetByGameMasterAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetByGameMasterAsync_ReturnsOnlyThatGameMastersCampaigns()
    {
        var gmId = Guid.NewGuid();
        var campaigns = new List<Campaign>
        {
            new() { Id = Guid.NewGuid(), Name = "Arc One", GameMasterId = gmId },
            new() { Id = Guid.NewGuid(), Name = "Arc Two", GameMasterId = gmId }
        };
        _campaignRepoMock.Setup(r => r.GetByGameMasterAsync(gmId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaigns);

        var result = await _sut.GetByGameMasterAsync(gmId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
    }

    // ── GetRosterAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetRosterAsync_ReturnsOnlyPlayersRecruitedByThatGameMaster()
    {
        var gmId = Guid.NewGuid();
        var mine = BuildPlayer(gmId);
        var someoneElses = BuildPlayer(Guid.NewGuid());
        _userManagerMock.Setup(m => m.Users).Returns(new[] { mine, someoneElses }.AsQueryable());

        var result = await _sut.GetRosterAsync(gmId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().ContainSingle(p => p.Id == mine.Id);
    }

    // ── AssignMemberAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task AssignMemberAsync_WhenCampaignNotOwnedByCaller_ReturnsNotFound()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var result = await _sut.AssignMemberAsync(
            gmId, campaign.Id, new AssignMemberRequest { PlayerId = Guid.NewGuid() });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Campaign.NotFound);
    }

    [Fact]
    public async Task AssignMemberAsync_WhenPlayerNotInRoster_ReturnsFailure()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var notMyPlayer = BuildPlayer(Guid.NewGuid());
        _userManagerMock.Setup(m => m.FindByIdAsync(notMyPlayer.Id.ToString()))
            .ReturnsAsync(notMyPlayer);

        var result = await _sut.AssignMemberAsync(
            gmId, campaign.Id, new AssignMemberRequest { PlayerId = notMyPlayer.Id });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Campaign.PlayerNotInRoster);
    }

    [Fact]
    public async Task AssignMemberAsync_WhenAlreadyMember_ReturnsFailure()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var player = BuildPlayer(gmId);
        _userManagerMock.Setup(m => m.FindByIdAsync(player.Id.ToString())).ReturnsAsync(player);
        _membershipRepoMock.Setup(r => r.ExistsAsync(campaign.Id, player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.AssignMemberAsync(
            gmId, campaign.Id, new AssignMemberRequest { PlayerId = player.Id });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Campaign.AlreadyMember);
    }

    [Fact]
    public async Task AssignMemberAsync_WithValidData_CreatesMembership()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var player = BuildPlayer(gmId);
        _userManagerMock.Setup(m => m.FindByIdAsync(player.Id.ToString())).ReturnsAsync(player);
        _membershipRepoMock.Setup(r => r.ExistsAsync(campaign.Id, player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _membershipRepoMock.Setup(r => r.AddAsync(It.IsAny<CampaignMembership>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _membershipRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.AssignMemberAsync(
            gmId, campaign.Id, new AssignMemberRequest { PlayerId = player.Id });

        result.IsSuccess.Should().BeTrue();
        result.Value!.PlayerId.Should().Be(player.Id);
        _membershipRepoMock.Verify(r => r.AddAsync(
            It.Is<CampaignMembership>(m => m.CampaignId == campaign.Id && m.PlayerId == player.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetMembersAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetMembersAsync_ReturnsMembersWithDisplayInfo()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var player = BuildPlayer(gmId);
        var membership = new CampaignMembership
        {
            Id = Guid.NewGuid(), CampaignId = campaign.Id, PlayerId = player.Id
        };
        _membershipRepoMock.Setup(r => r.GetByCampaignAsync(campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { membership });
        _userManagerMock.Setup(m => m.FindByIdAsync(player.Id.ToString())).ReturnsAsync(player);

        var result = await _sut.GetMembersAsync(gmId, campaign.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().ContainSingle(m => m.PlayerId == player.Id && m.DisplayName == player.DisplayName);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static ApplicationUser BuildPlayer(Guid recruitedBy) => new()
    {
        Id = Guid.NewGuid(),
        Email = Faker.Internet.Email(),
        UserName = Faker.Internet.UserName(),
        DisplayName = Faker.Name.FullName(),
        Role = UserRole.Player,
        RecruitedByGameMasterId = recruitedBy,
        CreatedAt = DateTime.UtcNow
    };
}
