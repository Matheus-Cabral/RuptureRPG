using FluentAssertions;
using Moq;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Infrastructure.Services;
using Ruptura.Shared.Catalog;

namespace Ruptura.UnitTests.Application;

public class CatalogEntryServiceTests
{
    private readonly Mock<ICatalogEntryRepository> _catalogRepoMock = new();
    private readonly Mock<ICampaignRepository> _campaignRepoMock = new();
    private readonly Mock<ICampaignMembershipRepository> _membershipRepoMock = new();
    private readonly CatalogEntryService _sut;

    public CatalogEntryServiceTests()
    {
        _sut = new CatalogEntryService(
            _catalogRepoMock.Object, _campaignRepoMock.Object, _membershipRepoMock.Object);
    }

    // ── GetByTypeAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByTypeAsync_WithInvalidType_ReturnsFailure()
    {
        var result = await _sut.GetByTypeAsync(Guid.NewGuid(), "NotARealType", Guid.NewGuid(), includeArchived: false);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.InvalidType);
    }

    [Fact]
    public async Task GetByTypeAsync_WhenCallerIsGameMaster_ReturnsEntries()
    {
        var gmId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };
        var entries = new List<CatalogEntry>
        {
            new() { Id = Guid.NewGuid(), Type = CatalogEntryType.Talent, Name = "Golpe Certeiro" }
        };

        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.GetByTypeAsync(CatalogEntryType.Talent, campaignId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var result = await _sut.GetByTypeAsync(gmId, "Talent", campaignId, includeArchived: false);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().ContainSingle(e => e.Name == "Golpe Certeiro");
        _membershipRepoMock.Verify(
            r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByTypeAsync_WhenCallerIsMember_ReturnsEntries()
    {
        var playerId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = Guid.NewGuid() };

        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _membershipRepoMock.Setup(r => r.ExistsAsync(campaignId, playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _catalogRepoMock.Setup(r => r.GetByTypeAsync(CatalogEntryType.Skill, campaignId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.GetByTypeAsync(playerId, "Skill", campaignId, includeArchived: false);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetByTypeAsync_WhenCallerIsMember_FiltersOutPrivateEntries()
    {
        var playerId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = Guid.NewGuid() };
        var entries = new List<CatalogEntry>
        {
            new() { Id = Guid.NewGuid(), Type = CatalogEntryType.Talent, Name = "Público", IsPublic = true },
            new() { Id = Guid.NewGuid(), Type = CatalogEntryType.Talent, Name = "Rascunho", IsPublic = false }
        };

        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _membershipRepoMock.Setup(r => r.ExistsAsync(campaignId, playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _catalogRepoMock.Setup(r => r.GetByTypeAsync(CatalogEntryType.Talent, campaignId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var result = await _sut.GetByTypeAsync(playerId, "Talent", campaignId, includeArchived: false);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().ContainSingle(e => e.Name == "Público");
    }

    [Fact]
    public async Task GetByTypeAsync_WhenCallerIsGameMaster_IncludesPrivateEntries()
    {
        var gmId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };
        var entries = new List<CatalogEntry>
        {
            new() { Id = Guid.NewGuid(), Type = CatalogEntryType.Talent, Name = "Público", IsPublic = true },
            new() { Id = Guid.NewGuid(), Type = CatalogEntryType.Talent, Name = "Rascunho", IsPublic = false }
        };

        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.GetByTypeAsync(CatalogEntryType.Talent, campaignId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var result = await _sut.GetByTypeAsync(gmId, "Talent", campaignId, includeArchived: false);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByTypeAsync_WhenCallerNotMember_ReturnsNotFound()
    {
        var strangerId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = Guid.NewGuid() };

        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _membershipRepoMock.Setup(r => r.ExistsAsync(campaignId, strangerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.GetByTypeAsync(strangerId, "Skill", campaignId, includeArchived: false);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.NotFound);
    }

    [Fact]
    public async Task GetByTypeAsync_WhenCampaignDoesNotExist_ReturnsNotFound()
    {
        var callerId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();

        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Campaign?)null);

        var result = await _sut.GetByTypeAsync(callerId, "Skill", campaignId, includeArchived: false);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.NotFound);
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WithInvalidType_ReturnsFailure()
    {
        var result = await _sut.CreateAsync(Guid.NewGuid(), new CreateCatalogEntryRequest
        {
            CampaignId = Guid.NewGuid(), Type = "NotARealType", Name = "X", DataJson = "{}"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.InvalidType);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesHomebrewEntry()
    {
        var gmId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };

        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.ExistsAsync(CatalogEntryType.Talent, campaignId, "Fôlego de Aço", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _catalogRepoMock.Setup(r => r.AddAsync(It.IsAny<CatalogEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _catalogRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.CreateAsync(gmId, new CreateCatalogEntryRequest
        {
            CampaignId = campaignId,
            Type = "Talent",
            Name = "Fôlego de Aço",
            DataJson = "{\"Category\":\"Combate\",\"Effect\":\"teste\",\"PowerTier\":\"menor\"}"
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsGlobal.Should().BeFalse();
        result.Value.CampaignId.Should().Be(campaignId);
        _catalogRepoMock.Verify(r => r.AddAsync(
            It.Is<CatalogEntry>(e => e.Name == "Fôlego de Aço" && e.CreatedByGameMasterId == gmId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithIsPublicFalse_CreatesPrivateEntry()
    {
        var gmId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };

        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.ExistsAsync(CatalogEntryType.Talent, campaignId, "Rascunho", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _catalogRepoMock.Setup(r => r.AddAsync(It.IsAny<CatalogEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _catalogRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.CreateAsync(gmId, new CreateCatalogEntryRequest
        {
            CampaignId = campaignId, Type = "Talent", Name = "Rascunho", DataJson = "{}", IsPublic = false
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsPublic.Should().BeFalse();
        _catalogRepoMock.Verify(r => r.AddAsync(
            It.Is<CatalogEntry>(e => !e.IsPublic), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenCampaignNotOwnedByCaller_ReturnsNotFound()
    {
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = Guid.NewGuid() };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var result = await _sut.CreateAsync(Guid.NewGuid(), new CreateCatalogEntryRequest
        {
            CampaignId = campaignId, Type = "Talent", Name = "X", DataJson = "{}"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.NotFound);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExistsInScope_ReturnsAlreadyExists()
    {
        var gmId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.ExistsAsync(CatalogEntryType.Talent, campaignId, "Duplicado", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.CreateAsync(gmId, new CreateCatalogEntryRequest
        {
            CampaignId = campaignId, Type = "Talent", Name = "Duplicado", DataJson = "{}"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.AlreadyExists);
    }

    [Fact]
    public async Task CreateAsync_WhenNameCollidesWithOfficialEntry_ReturnsAlreadyExists()
    {
        var gmId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.ExistsAsync(CatalogEntryType.Talent, campaignId, "Golpe Certeiro", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _catalogRepoMock.Setup(r => r.ExistsAsync(CatalogEntryType.Talent, null, "Golpe Certeiro", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.CreateAsync(gmId, new CreateCatalogEntryRequest
        {
            CampaignId = campaignId, Type = "Talent", Name = "Golpe Certeiro", DataJson = "{}"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.AlreadyExists);
    }

    // ── UpdateAsync / DeleteAsync ────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_OnGlobalEntry_ReturnsCannotModifyGlobalEntry()
    {
        var entry = new CatalogEntry { Id = Guid.NewGuid(), Type = CatalogEntryType.Origin, CampaignId = null, Name = "Soldado" };
        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), entry.Id, new UpdateCatalogEntryRequest
        {
            Name = "Soldado Editado", DataJson = "{}"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.CannotModifyGlobalEntry);
    }

    [Fact]
    public async Task UpdateAsync_WhenCallerDoesNotOwnCampaign_ReturnsNotFound()
    {
        var campaignId = Guid.NewGuid();
        var entry = new CatalogEntry { Id = Guid.NewGuid(), Type = CatalogEntryType.Talent, CampaignId = campaignId, Name = "X" };
        var campaign = new Campaign { Id = campaignId, GameMasterId = Guid.NewGuid() };

        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), entry.Id, new UpdateCatalogEntryRequest
        {
            Name = "Y", DataJson = "{}"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesEntry()
    {
        var gmId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var entry = new CatalogEntry { Id = Guid.NewGuid(), Type = CatalogEntryType.Talent, CampaignId = campaignId, Name = "Old" };
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };

        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.ExistsAsync(CatalogEntryType.Talent, campaignId, "New", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _catalogRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(gmId, entry.Id, new UpdateCatalogEntryRequest
        {
            Name = "New", DataJson = "{\"a\":1}"
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("New");
        entry.Name.Should().Be("New");
    }

    [Fact]
    public async Task UpdateAsync_TogglesIsPublic()
    {
        var gmId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var entry = new CatalogEntry
        {
            Id = Guid.NewGuid(), Type = CatalogEntryType.Talent, CampaignId = campaignId, Name = "X", IsPublic = true
        };
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };

        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.ExistsAsync(CatalogEntryType.Talent, campaignId, "X", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _catalogRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(gmId, entry.Id, new UpdateCatalogEntryRequest
        {
            Name = "X", DataJson = "{}", IsPublic = false
        });

        result.IsSuccess.Should().BeTrue();
        entry.IsPublic.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ReturnsAlreadyExists()
    {
        var gmId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var entry = new CatalogEntry { Id = Guid.NewGuid(), Type = CatalogEntryType.Talent, CampaignId = campaignId, Name = "Old" };
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };

        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.ExistsAsync(CatalogEntryType.Talent, campaignId, "Duplicado", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.UpdateAsync(gmId, entry.Id, new UpdateCatalogEntryRequest
        {
            Name = "Duplicado", DataJson = "{}"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.AlreadyExists);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntryNotFound_ReturnsNotFound()
    {
        var entryId = Guid.NewGuid();
        _catalogRepoMock.Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CatalogEntry?)null);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), entryId, new UpdateCatalogEntryRequest
        {
            Name = "Y", DataJson = "{}"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_OnGlobalEntry_ReturnsCannotModifyGlobalEntry()
    {
        var entry = new CatalogEntry { Id = Guid.NewGuid(), Type = CatalogEntryType.Skill, CampaignId = null, Name = "Espadas" };
        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), entry.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.CannotModifyGlobalEntry);
    }

    [Fact]
    public async Task DeleteAsync_WithValidData_ArchivesEntry()
    {
        var gmId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var entry = new CatalogEntry { Id = Guid.NewGuid(), Type = CatalogEntryType.Talent, CampaignId = campaignId, Name = "X" };
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };

        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(gmId, entry.Id);

        result.IsSuccess.Should().BeTrue();
        _catalogRepoMock.Verify(r => r.Update(entry), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenEntryNotFound_ReturnsNotFound()
    {
        var entryId = Guid.NewGuid();
        _catalogRepoMock.Setup(r => r.GetByIdAsync(entryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CatalogEntry?)null);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), entryId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_ArchivesTheEntryInsteadOfRemovingIt()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var entry = new CatalogEntry { Id = Guid.NewGuid(), CampaignId = campaign.Id, Type = CatalogEntryType.Talent, Name = "Homebrew Talent" };
        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(gmId, entry.Id);

        result.IsSuccess.Should().BeTrue();
        entry.IsArchived.Should().BeTrue();
        _catalogRepoMock.Verify(r => r.Remove(It.IsAny<CatalogEntry>()), Times.Never);
        _catalogRepoMock.Verify(r => r.Update(It.Is<CatalogEntry>(e => e.IsArchived)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenAlreadyArchived_ReturnsFailure()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var entry = new CatalogEntry { Id = Guid.NewGuid(), CampaignId = campaign.Id, Type = CatalogEntryType.Talent, Name = "X", IsArchived = true };
        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.DeleteAsync(gmId, entry.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.AlreadyArchived);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntryIsArchived_ReturnsFailure()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var entry = new CatalogEntry { Id = Guid.NewGuid(), CampaignId = campaign.Id, Type = CatalogEntryType.Talent, Name = "X", IsArchived = true };
        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.UpdateAsync(gmId, entry.Id, new UpdateCatalogEntryRequest { Name = "Y", DataJson = "{}" });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.AlreadyArchived);
    }

    [Fact]
    public async Task GetByTypeAsync_WithIncludeArchivedFalse_PassesFalseToRepository()
    {
        var callerId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = callerId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.GetByTypeAsync(CatalogEntryType.Talent, campaign.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.GetByTypeAsync(callerId, "Talent", campaign.Id, includeArchived: false);

        result.IsSuccess.Should().BeTrue();
        _catalogRepoMock.Verify(r => r.GetByTypeAsync(CatalogEntryType.Talent, campaign.Id, false, It.IsAny<CancellationToken>()), Times.Once);
    }
}
