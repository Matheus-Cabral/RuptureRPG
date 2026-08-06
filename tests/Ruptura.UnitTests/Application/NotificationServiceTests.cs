using FluentAssertions;
using Moq;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Infrastructure.Services;

namespace Ruptura.UnitTests.Application;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepoMock = new();
    private readonly Mock<ICampaignRepository> _campaignRepoMock = new();
    private readonly Mock<ICharacterSheetRepository> _sheetRepoMock = new();
    private readonly Mock<ICharacterSheetService> _characterSheetServiceMock = new();
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _sut = new NotificationService(
            _notificationRepoMock.Object, _campaignRepoMock.Object,
            _sheetRepoMock.Object, _characterSheetServiceMock.Object);
    }

    // ── CheckAndCreateRankPromotionNotificationAsync ────────────────────────────

    [Fact]
    public async Task CheckAndCreate_WhenNpDoesNotExceedCeiling_DoesNotCreateNotification()
    {
        var result = await _sut.CheckAndCreateRankPromotionNotificationAsync(
            Guid.NewGuid(), Guid.NewGuid(), "Bronze", 70);

        result.IsSuccess.Should().BeTrue();
        _notificationRepoMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckAndCreate_WhenNpExceedsCeilingAndNoneExists_CreatesNotificationForCampaignGameMaster()
    {
        var campaignId = Guid.NewGuid();
        var sheetId = Guid.NewGuid();
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _notificationRepoMock.Setup(r => r.ExistsUnreadForSheetAsync(
                sheetId, NotificationType.RankPromotionAvailable, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _notificationRepoMock.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _notificationRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.CheckAndCreateRankPromotionNotificationAsync(campaignId, sheetId, "Bronze", 71);

        result.IsSuccess.Should().BeTrue();
        _notificationRepoMock.Verify(r => r.AddAsync(
            It.Is<Notification>(n =>
                n.RecipientUserId == gmId && n.CampaignId == campaignId
                && n.RelatedCharacterSheetId == sheetId && n.Type == NotificationType.RankPromotionAvailable
                && !n.IsRead),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckAndCreate_WhenUnreadNotificationAlreadyExists_DoesNotCreateDuplicate()
    {
        var campaignId = Guid.NewGuid();
        var sheetId = Guid.NewGuid();
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Campaign { Id = campaignId, GameMasterId = Guid.NewGuid() });
        _notificationRepoMock.Setup(r => r.ExistsUnreadForSheetAsync(
                sheetId, NotificationType.RankPromotionAvailable, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.CheckAndCreateRankPromotionNotificationAsync(campaignId, sheetId, "Bronze", 90);

        result.IsSuccess.Should().BeTrue();
        _notificationRepoMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckAndCreate_AtLendario_NeverTriggersEvenWithHugeNp()
    {
        var result = await _sut.CheckAndCreateRankPromotionNotificationAsync(
            Guid.NewGuid(), Guid.NewGuid(), "Lendário", 10_000);

        result.IsSuccess.Should().BeTrue();
        _notificationRepoMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckAndCreate_WhenCampaignNoLongerExists_IsANoOp()
    {
        var campaignId = Guid.NewGuid();
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Campaign?)null);

        var result = await _sut.CheckAndCreateRankPromotionNotificationAsync(campaignId, Guid.NewGuid(), "Bronze", 90);

        result.IsSuccess.Should().BeTrue();
        _notificationRepoMock.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckAndCreate_WhenNpNoLongerExceedsCeiling_MarksExistingNotificationRead()
    {
        var characterSheetId = Guid.NewGuid();

        var result = await _sut.CheckAndCreateRankPromotionNotificationAsync(
            Guid.NewGuid(), characterSheetId, "Ferro", 90);

        result.IsSuccess.Should().BeTrue();
        _notificationRepoMock.Verify(r => r.MarkReadForSheetAsync(
            characterSheetId, NotificationType.RankPromotionAvailable, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── PromoteAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task PromoteAsync_AdvancesExactlyOneRankAndMarksRead()
    {
        var gmId = Guid.NewGuid();
        var sheetId = Guid.NewGuid();
        var notification = new Notification
        {
            Id = Guid.NewGuid(), RecipientUserId = gmId, RelatedCharacterSheetId = sheetId, IsRead = false
        };
        _notificationRepoMock.Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>())).ReturnsAsync(notification);
        _characterSheetServiceMock.Setup(s => s.GetRankingAsync(sheetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("Bronze"));
        _characterSheetServiceMock.Setup(s => s.SetRankingAsync(sheetId, "Ferro", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _notificationRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.PromoteAsync(gmId, notification.Id);

        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        _characterSheetServiceMock.Verify(s => s.SetRankingAsync(sheetId, "Ferro", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PromoteAsync_WhenCallerIsNotTheRecipient_ReturnsNotFound()
    {
        var notification = new Notification { Id = Guid.NewGuid(), RecipientUserId = Guid.NewGuid() };
        _notificationRepoMock.Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>())).ReturnsAsync(notification);

        var result = await _sut.PromoteAsync(Guid.NewGuid(), notification.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Notification.NotFound);
    }

    [Fact]
    public async Task PromoteAsync_WhenNotRelatedToASheet_ReturnsNotPromotable()
    {
        var gmId = Guid.NewGuid();
        var notification = new Notification { Id = Guid.NewGuid(), RecipientUserId = gmId, RelatedCharacterSheetId = null };
        _notificationRepoMock.Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>())).ReturnsAsync(notification);

        var result = await _sut.PromoteAsync(gmId, notification.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Notification.NotPromotable);
    }

    [Fact]
    public async Task PromoteAsync_WhenAlreadyAtLendario_ReturnsNotPromotable()
    {
        var gmId = Guid.NewGuid();
        var sheetId = Guid.NewGuid();
        var notification = new Notification
        {
            Id = Guid.NewGuid(), RecipientUserId = gmId, RelatedCharacterSheetId = sheetId
        };
        _notificationRepoMock.Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>())).ReturnsAsync(notification);
        _characterSheetServiceMock.Setup(s => s.GetRankingAsync(sheetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("Lendário"));

        var result = await _sut.PromoteAsync(gmId, notification.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Notification.NotPromotable);
        _characterSheetServiceMock.Verify(
            s => s.SetRankingAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PromoteAsync_WhenAlreadyRead_ReturnsNotFound()
    {
        var gmId = Guid.NewGuid();
        var notification = new Notification
        {
            Id = Guid.NewGuid(), RecipientUserId = gmId, RelatedCharacterSheetId = Guid.NewGuid(), IsRead = true
        };
        _notificationRepoMock.Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>())).ReturnsAsync(notification);

        var result = await _sut.PromoteAsync(gmId, notification.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Notification.NotFound);
        _characterSheetServiceMock.Verify(
            s => s.SetRankingAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── DismissAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DismissAsync_MarksReadWithoutChangingRanking()
    {
        var gmId = Guid.NewGuid();
        var notification = new Notification { Id = Guid.NewGuid(), RecipientUserId = gmId, IsRead = false };
        _notificationRepoMock.Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>())).ReturnsAsync(notification);
        _notificationRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.DismissAsync(gmId, notification.Id);

        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        _characterSheetServiceMock.Verify(
            s => s.SetRankingAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DismissAsync_WhenCallerIsNotTheRecipient_ReturnsNotFound()
    {
        var notification = new Notification { Id = Guid.NewGuid(), RecipientUserId = Guid.NewGuid() };
        _notificationRepoMock.Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>())).ReturnsAsync(notification);

        var result = await _sut.DismissAsync(Guid.NewGuid(), notification.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Notification.NotFound);
    }

    [Fact]
    public async Task DismissAsync_WhenAlreadyRead_ReturnsNotFound()
    {
        var gmId = Guid.NewGuid();
        var notification = new Notification { Id = Guid.NewGuid(), RecipientUserId = gmId, IsRead = true };
        _notificationRepoMock.Setup(r => r.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>())).ReturnsAsync(notification);

        var result = await _sut.DismissAsync(gmId, notification.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Notification.NotFound);
    }

    // ── GetForGameMasterAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetForGameMasterAsync_GroupsUnreadNotificationsByCampaign()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), Name = "Test Campaign", GameMasterId = gmId };
        var sheet = new CharacterSheet { Id = Guid.NewGuid(), CharacterName = "Sir Aldric" };
        var notification = new Notification
        {
            Id = Guid.NewGuid(), RecipientUserId = gmId, CampaignId = campaign.Id,
            RelatedCharacterSheetId = sheet.Id, Type = NotificationType.RankPromotionAvailable, IsRead = false
        };
        _notificationRepoMock.Setup(r => r.GetUnreadByRecipientAsync(gmId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([notification]);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);

        var result = await _sut.GetForGameMasterAsync(gmId);

        result.IsSuccess.Should().BeTrue();
        var groups = result.Value!.ToList();
        groups.Should().ContainSingle();
        groups[0].CampaignId.Should().Be(campaign.Id);
        groups[0].CampaignName.Should().Be("Test Campaign");
        groups[0].Notifications.Should().ContainSingle(n => n.CharacterName == "Sir Aldric" && n.Type == "RankPromotionAvailable");
    }

    [Fact]
    public async Task GetForGameMasterAsync_WhenNoUnreadNotifications_ReturnsEmpty()
    {
        _notificationRepoMock.Setup(r => r.GetUnreadByRecipientAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.GetForGameMasterAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }
}
