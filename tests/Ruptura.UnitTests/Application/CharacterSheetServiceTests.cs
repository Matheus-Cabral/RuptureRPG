using System.Text.Json;
using Bogus;
using FluentAssertions;
using Moq;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Services;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.UnitTests.Application;

public class CharacterSheetServiceTests
{
    private readonly Mock<ICharacterSheetRepository> _sheetRepoMock = new();
    private readonly Mock<ICampaignRepository> _campaignRepoMock = new();
    private readonly Mock<ICampaignMembershipRepository> _membershipRepoMock = new();
    private readonly Mock<ICatalogEntryRepository> _catalogRepoMock = new();
    private readonly Mock<ICharacterStatsCalculator> _calculatorMock = new();
    private readonly CharacterSheetService _sut;

    private static readonly Faker Faker = new();

    public CharacterSheetServiceTests()
    {
        _calculatorMock
            .Setup(c => c.Calculate(It.IsAny<CharacterSheetData>(), It.IsAny<IReadOnlyDictionary<Guid, CatalogEntry>>()))
            .Returns(new CharacterDerivedStats());

        _sut = new CharacterSheetService(
            _sheetRepoMock.Object, _campaignRepoMock.Object, _membershipRepoMock.Object,
            _catalogRepoMock.Object, _calculatorMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenCampaignNotOwnedByCaller_ReturnsNotFound()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.CreateAsync(
            gmId, campaign.Id, new GrantCharacterSheetRequest { PlayerId = Guid.NewGuid(), CharacterName = "X" });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.NotFound);
    }

    [Fact]
    public async Task CreateAsync_WhenPlayerNotCampaignMember_ReturnsFailure()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        var playerId = Guid.NewGuid();
        _membershipRepoMock.Setup(r => r.ExistsAsync(campaign.Id, playerId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.CreateAsync(
            gmId, campaign.Id, new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "X" });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.PlayerNotMember);
    }

    [Fact]
    public async Task CreateAsync_WhenPlayerAlreadyHasAliveCharacterInCampaign_ReturnsFailure()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var playerId = Guid.NewGuid();
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _membershipRepoMock.Setup(r => r.ExistsAsync(campaign.Id, playerId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _sheetRepoMock.Setup(r => r.GetAliveByOwnerAndCampaignAsync(playerId, campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CharacterSheet { Id = Guid.NewGuid() });

        var result = await _sut.CreateAsync(
            gmId, campaign.Id, new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "X" });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.AlreadyHasAliveCharacter);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_PersistsSheetWithEmptyDefaultData()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var playerId = Guid.NewGuid();
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _membershipRepoMock.Setup(r => r.ExistsAsync(campaign.Id, playerId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _sheetRepoMock.Setup(r => r.GetAliveByOwnerAndCampaignAsync(playerId, campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CharacterSheet?)null);
        _sheetRepoMock.Setup(r => r.AddAsync(It.IsAny<CharacterSheet>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _sheetRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.CreateAsync(
            gmId, campaign.Id, new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });

        result.IsSuccess.Should().BeTrue();
        result.Value!.CharacterName.Should().Be("Sir Aldric");
        result.Value.OwnerId.Should().Be(playerId);
        result.Value.CampaignId.Should().Be(campaign.Id);
        result.Value.GrantedByGameMasterId.Should().Be(gmId);
        _sheetRepoMock.Verify(r => r.AddAsync(
            It.Is<CharacterSheet>(s => s.OwnerId == playerId && s.CampaignId == campaign.Id && !s.IsDead && !s.IsRetired),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
