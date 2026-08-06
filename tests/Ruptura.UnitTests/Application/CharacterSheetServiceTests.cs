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

    // ── AuthorizeAccessAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task AuthorizeAccessAsync_AsOwner_Succeeds()
    {
        var ownerId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = ownerId, CampaignId = campaign.Id };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.AuthorizeAccessAsync(ownerId, sheet.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(sheet.Id);
    }

    [Fact]
    public async Task AuthorizeAccessAsync_AsUnrelatedCaller_ReturnsNotFound()
    {
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CampaignId = campaign.Id };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.AuthorizeAccessAsync(Guid.NewGuid(), sheet.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.NotFound);
    }

    // ── SetPortraitPathAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task SetPortraitPathAsync_UpdatesThePortraitPath()
    {
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), PortraitImagePath = "old.jpg" };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _sheetRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.SetPortraitPathAsync(sheet.Id, "new.jpg");

        result.IsSuccess.Should().BeTrue();
        sheet.PortraitImagePath.Should().Be("new.jpg");
        _sheetRepoMock.Verify(r => r.Update(sheet), Times.Once);
    }

    // ── GetRankingAsync / SetRankingAsync ───────────────────────────────────────

    [Fact]
    public async Task GetRankingAsync_ReturnsCurrentRanking()
    {
        var data = new CharacterSheetData();
        data.GuildRegistry.Ranking = "Ferro";
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), DataJson = JsonSerializer.Serialize(data) };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);

        var result = await _sut.GetRankingAsync(sheet.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Ferro");
    }

    [Fact]
    public async Task GetRankingAsync_WhenSheetDoesNotExist_ReturnsNotFound()
    {
        _sheetRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CharacterSheet?)null);

        var result = await _sut.GetRankingAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.NotFound);
    }

    [Fact]
    public async Task SetRankingAsync_UpdatesTheRankingInDataJson()
    {
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), DataJson = JsonSerializer.Serialize(new CharacterSheetData()) };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _sheetRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.SetRankingAsync(sheet.Id, "Ferro");

        result.IsSuccess.Should().BeTrue();
        JsonSerializer.Deserialize<CharacterSheetData>(sheet.DataJson)!.GuildRegistry.Ranking.Should().Be("Ferro");
        _sheetRepoMock.Verify(r => r.Update(sheet), Times.Once);
    }

    [Fact]
    public async Task SetRankingAsync_WhenSheetDoesNotExist_ReturnsNotFound()
    {
        _sheetRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CharacterSheet?)null);

        var result = await _sut.SetRankingAsync(Guid.NewGuid(), "Ferro");

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

    // ── UpdateAsync ──────────────────────────────────────────────────────────

    private static CharacterSheet BuildAliveSheet(Guid ownerId, Guid campaignId) => new()
    {
        Id = Guid.NewGuid(), OwnerId = ownerId, CampaignId = campaignId, CharacterName = "Old Name",
        IsDead = false, IsRetired = false, DataJson = JsonSerializer.Serialize(new CharacterSheetData())
    };

    [Fact]
    public async Task UpdateAsync_AsOwner_UpdatesGeneralFieldsWithoutTouchingStatus()
    {
        var ownerId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = BuildAliveSheet(ownerId, campaign.Id);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _sheetRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(ownerId, sheet.Id, new UpdateCharacterSheetRequest
        {
            CharacterName = "New Name", DataJson = JsonSerializer.Serialize(new CharacterSheetData()),
            IsDead = false, IsRetired = false
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.CharacterName.Should().Be("New Name");
        sheet.IsDead.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_AsOwnerAttemptingToMarkDead_ReturnsFailureAndDoesNotSave()
    {
        var ownerId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = BuildAliveSheet(ownerId, campaign.Id);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.UpdateAsync(ownerId, sheet.Id, new UpdateCharacterSheetRequest
        {
            CharacterName = "New Name", DataJson = JsonSerializer.Serialize(new CharacterSheetData()),
            IsDead = true, IsRetired = false
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.OnlyGameMasterCanChangeStatus);
        sheet.CharacterName.Should().Be("Old Name");
        _sheetRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_AsGameMaster_CanMarkCharacterDead()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var sheet = BuildAliveSheet(Guid.NewGuid(), campaign.Id);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _sheetRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(gmId, sheet.Id, new UpdateCharacterSheetRequest
        {
            CharacterName = "Old Name", DataJson = JsonSerializer.Serialize(new CharacterSheetData()),
            IsDead = true, IsRetired = false
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsDead.Should().BeTrue();
        sheet.IsDead.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_AsUnrelatedCaller_ReturnsNotFound()
    {
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = BuildAliveSheet(Guid.NewGuid(), campaign.Id);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), sheet.Id, new UpdateCharacterSheetRequest
        {
            CharacterName = "X", DataJson = JsonSerializer.Serialize(new CharacterSheetData())
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WhenSaveViolatesUniqueAliveIndex_ReturnsAlreadyHasAliveCharacter()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var sheet = new CharacterSheet
        {
            Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CampaignId = campaign.Id, CharacterName = "Resurrected",
            IsDead = true, IsRetired = false, DataJson = JsonSerializer.Serialize(new CharacterSheetData())
        };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _sheetRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateException("unique violation"));

        // GM tries to un-kill this character back to alive, while another alive sheet
        // for the same owner+campaign already exists (simulated by the DB throwing).
        var result = await _sut.UpdateAsync(gmId, sheet.Id, new UpdateCharacterSheetRequest
        {
            CharacterName = "Resurrected", DataJson = JsonSerializer.Serialize(new CharacterSheetData()),
            IsDead = false, IsRetired = false
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.AlreadyHasAliveCharacter);
    }
}
