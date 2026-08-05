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

    // ── GetAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_AsOwner_ReturnsSheet()
    {
        var ownerId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = new CharacterSheet
        {
            Id = Guid.NewGuid(), OwnerId = ownerId, CampaignId = campaign.Id,
            DataJson = JsonSerializer.Serialize(new CharacterSheetData())
        };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.GetAsync(ownerId, sheet.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(sheet.Id);
    }

    [Fact]
    public async Task GetAsync_AsCampaignGameMaster_ReturnsSheet()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var sheet = new CharacterSheet
        {
            Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CampaignId = campaign.Id,
            DataJson = JsonSerializer.Serialize(new CharacterSheetData())
        };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.GetAsync(gmId, sheet.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_AsUnrelatedCaller_ReturnsNotFound()
    {
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = new CharacterSheet
        {
            Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CampaignId = campaign.Id,
            DataJson = JsonSerializer.Serialize(new CharacterSheetData())
        };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.GetAsync(Guid.NewGuid(), sheet.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.NotFound);
    }

    [Fact]
    public async Task GetAsync_WhenSheetDoesNotExist_ReturnsNotFound()
    {
        _sheetRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CharacterSheet?)null);

        var result = await _sut.GetAsync(Guid.NewGuid(), Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.NotFound);
    }

    // ── GetByCampaignAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetByCampaignAsync_AsOwningGameMaster_ReturnsAllSheetsInCampaign()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var sheets = new List<CharacterSheet>
        {
            new() { Id = Guid.NewGuid(), CampaignId = campaign.Id, DataJson = JsonSerializer.Serialize(new CharacterSheetData()) }
        };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _sheetRepoMock.Setup(r => r.GetByCampaignAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheets);

        var result = await _sut.GetByCampaignAsync(gmId, campaign.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByCampaignAsync_WhenCallerIsNotTheGameMaster_ReturnsNotFound()
    {
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.GetByCampaignAsync(Guid.NewGuid(), campaign.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.NotFound);
    }

    // ── GetMineAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMineAsync_WhenPlayerHasAnAliveCharacterInCampaign_ReturnsIt()
    {
        var playerId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var sheet = new CharacterSheet
        {
            Id = Guid.NewGuid(), OwnerId = playerId, CampaignId = campaignId,
            DataJson = JsonSerializer.Serialize(new CharacterSheetData())
        };
        _sheetRepoMock.Setup(r => r.GetAliveByOwnerAndCampaignAsync(playerId, campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sheet);

        var result = await _sut.GetMineAsync(playerId, campaignId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(sheet.Id);
    }

    [Fact]
    public async Task GetMineAsync_WhenNoCharacterGrantedYet_ReturnsNotFound()
    {
        _sheetRepoMock.Setup(r => r.GetAliveByOwnerAndCampaignAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CharacterSheet?)null);

        var result = await _sut.GetMineAsync(Guid.NewGuid(), Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.NotFound);
    }
}
