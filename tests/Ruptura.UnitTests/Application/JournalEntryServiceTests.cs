using Bogus;
using FluentAssertions;
using Moq;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Services;
using Ruptura.Shared.Journal;

namespace Ruptura.UnitTests.Application;

public class JournalEntryServiceTests
{
    private readonly Mock<ICharacterJournalEntryRepository> _journalRepoMock = new();
    private readonly Mock<ICharacterSheetRepository> _sheetRepoMock = new();
    private readonly Mock<ICampaignRepository> _campaignRepoMock = new();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly JournalEntryService _sut;

    private static readonly Faker Faker = new();

    public JournalEntryServiceTests()
    {
        _sut = new JournalEntryService(
            _journalRepoMock.Object, _sheetRepoMock.Object, _campaignRepoMock.Object, _fileStorageMock.Object);
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_AsOwner_PersistsEntryWithEmptyImagePaths()
    {
        var ownerId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = ownerId };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _journalRepoMock.Setup(r => r.AddAsync(It.IsAny<CharacterJournalEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _journalRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.CreateAsync(ownerId, sheet.Id, new CreateJournalEntryRequest { Text = "Day one." });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Text.Should().Be("Day one.");
        result.Value.ImagePaths.Should().BeEmpty();
        _journalRepoMock.Verify(r => r.AddAsync(
            It.Is<CharacterJournalEntry>(e => e.CharacterSheetId == sheet.Id && e.ImagePaths.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_AsNonOwner_ReturnsNotFound()
    {
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid() };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);

        var result = await _sut.CreateAsync(Guid.NewGuid(), sheet.Id, new CreateJournalEntryRequest { Text = "x" });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Journal.NotFound);
    }

    [Fact]
    public async Task CreateAsync_AsGameMaster_ReturnsNotFound()
    {
        // GM does not get to write the journal — only the owner does (design spec §6).
        var gmId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid() };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);

        var result = await _sut.CreateAsync(gmId, sheet.Id, new CreateJournalEntryRequest { Text = "x" });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Journal.NotFound);
    }

    // ── GetByCharacterSheetAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetByCharacterSheetAsync_AsOwner_ReturnsEntries()
    {
        var ownerId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = ownerId, CampaignId = campaign.Id };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _journalRepoMock.Setup(r => r.GetByCharacterSheetAsync(sheet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new CharacterJournalEntry { Id = Guid.NewGuid(), CharacterSheetId = sheet.Id, Text = "x" }]);

        var result = await _sut.GetByCharacterSheetAsync(ownerId, sheet.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByCharacterSheetAsync_AsCampaignGameMaster_ReturnsEntries()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CampaignId = campaign.Id };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _journalRepoMock.Setup(r => r.GetByCharacterSheetAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await _sut.GetByCharacterSheetAsync(gmId, sheet.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetByCharacterSheetAsync_AsUnrelatedCaller_ReturnsNotFound()
    {
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CampaignId = campaign.Id };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.GetByCharacterSheetAsync(Guid.NewGuid(), sheet.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Journal.NotFound);
    }

    // ── AuthorizeReadAsync / AuthorizeWriteAsync ─────────────────────────────

    [Fact]
    public async Task AuthorizeReadAsync_AsCampaignGameMaster_Succeeds()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CampaignId = campaign.Id };
        var entry = new CharacterJournalEntry { Id = Guid.NewGuid(), CharacterSheetId = sheet.Id };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.AuthorizeReadAsync(gmId, entry.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeWriteAsync_AsCampaignGameMaster_ReturnsNotFound()
    {
        // GM can read journal images but never write them — see design spec §6.
        var gmId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid() };
        var entry = new CharacterJournalEntry { Id = Guid.NewGuid(), CharacterSheetId = sheet.Id };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);

        var result = await _sut.AuthorizeWriteAsync(gmId, entry.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Journal.NotFound);
    }

    [Fact]
    public async Task AuthorizeWriteAsync_AsOwner_Succeeds()
    {
        var ownerId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = ownerId };
        var entry = new CharacterJournalEntry { Id = Guid.NewGuid(), CharacterSheetId = sheet.Id };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);

        var result = await _sut.AuthorizeWriteAsync(ownerId, entry.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(entry.Id);
    }

    // ── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_AsOwner_ReplacesTextAndImagePaths()
    {
        var ownerId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = ownerId };
        var entry = new CharacterJournalEntry
        {
            Id = Guid.NewGuid(), CharacterSheetId = sheet.Id, Text = "Old", ImagePaths = ["a.jpg", "b.jpg"]
        };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _journalRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(ownerId, entry.Id, new UpdateJournalEntryRequest
        {
            Text = "New", ImagePaths = ["a.jpg"]
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Text.Should().Be("New");
        result.Value.ImagePaths.Should().ContainSingle().Which.Should().Be("a.jpg");
    }

    [Fact]
    public async Task UpdateAsync_WhenAnImageIsDropped_DeletesItsFileFromDisk()
    {
        var ownerId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = ownerId };
        var entry = new CharacterJournalEntry
        {
            Id = Guid.NewGuid(), CharacterSheetId = sheet.Id, Text = "x", ImagePaths = ["a.jpg", "b.jpg"]
        };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _journalRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.UpdateAsync(ownerId, entry.Id, new UpdateJournalEntryRequest { Text = "x", ImagePaths = ["a.jpg"] });

        _fileStorageMock.Verify(f => f.DeleteAsync("b.jpg", It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageMock.Verify(f => f.DeleteAsync("a.jpg", It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_AsCampaignGameMaster_ReturnsNotFound()
    {
        var gmId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid() };
        var entry = new CharacterJournalEntry { Id = Guid.NewGuid(), CharacterSheetId = sheet.Id, Text = "x" };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);

        var result = await _sut.UpdateAsync(gmId, entry.Id, new UpdateJournalEntryRequest { Text = "y", ImagePaths = [] });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Journal.NotFound);
    }

    // ── AppendImagePathAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task AppendImagePathAsync_AddsThePathToTheEntrysImagePaths()
    {
        var entry = new CharacterJournalEntry
        {
            Id = Guid.NewGuid(), CharacterSheetId = Guid.NewGuid(), Text = "x", ImagePaths = ["existing.jpg"]
        };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _journalRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.AppendImagePathAsync(entry.Id, "new.jpg");

        result.IsSuccess.Should().BeTrue();
        entry.ImagePaths.Should().BeEquivalentTo(["existing.jpg", "new.jpg"]);
        _journalRepoMock.Verify(r => r.Update(entry), Times.Once);
    }

    [Fact]
    public async Task AppendImagePathAsync_WhenEntryDoesNotExist_ReturnsNotFound()
    {
        _journalRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CharacterJournalEntry?)null);

        var result = await _sut.AppendImagePathAsync(Guid.NewGuid(), "x.jpg");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Journal.NotFound);
    }

    // ── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_AsOwner_RemovesEntryAndDeletesAllImageFiles()
    {
        var ownerId = Guid.NewGuid();
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = ownerId };
        var entry = new CharacterJournalEntry
        {
            Id = Guid.NewGuid(), CharacterSheetId = sheet.Id, Text = "x", ImagePaths = ["a.jpg", "b.jpg"]
        };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _journalRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(ownerId, entry.Id);

        result.IsSuccess.Should().BeTrue();
        _fileStorageMock.Verify(f => f.DeleteAsync("a.jpg", It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageMock.Verify(f => f.DeleteAsync("b.jpg", It.IsAny<CancellationToken>()), Times.Once);
        _journalRepoMock.Verify(r => r.Remove(entry), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_AsUnrelatedCaller_ReturnsNotFound()
    {
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), OwnerId = Guid.NewGuid() };
        var entry = new CharacterJournalEntry { Id = Guid.NewGuid(), CharacterSheetId = sheet.Id, Text = "x" };
        _journalRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), entry.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Journal.NotFound);
    }
}
