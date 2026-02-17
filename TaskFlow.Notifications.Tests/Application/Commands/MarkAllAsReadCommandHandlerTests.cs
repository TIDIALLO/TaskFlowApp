using FluentAssertions;
using Moq;
using TaskFlow.Notifications.Application.Commands.MarkAllAsRead;
using TaskFlow.Notifications.Application.Interfaces;
using TaskFlow.Notifications.Domain.Entities;
using TaskFlow.Notifications.Tests.Fixtures;

namespace TaskFlow.Notifications.Tests.Application.Commands;

/// <summary>
/// Tests pour MarkAllAsReadCommandHandler.
/// 
/// SCÉNARIOS TESTÉS :
/// 1. Plusieurs notifications non lues → toutes marquées, retourne le count
/// 2. Mix lues/non lues → seules les non-lues sont marquées
/// 3. Aucune notification → succès avec count = 0
/// 4. Toutes déjà lues → succès avec count = 0
/// </summary>
public class MarkAllAsReadCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly Mock<INotificationUnitOfWork> _unitOfWorkMock;
    private readonly MarkAllAsReadCommandHandler _handler;

    public MarkAllAsReadCommandHandlerTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _unitOfWorkMock = new Mock<INotificationUnitOfWork>();

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _handler = new MarkAllAsReadCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithMixedNotifications_ShouldMarkOnlyUnreadOnes()
    {
        // Arrange — 3 notifications : 2 non lues, 1 lue
        var notifications = NotificationFixtures.CreateMixedList();
        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(NotificationFixtures.ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        var command = new MarkAllAsReadCommand(NotificationFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2); // 2 étaient non lues

        // Toutes les notifications doivent maintenant être lues
        notifications.Should().OnlyContain(n => n.IsRead);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoNotifications_ShouldReturnZero()
    {
        // Arrange — aucune notification
        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(NotificationFixtures.ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Notification>());

        var command = new MarkAllAsReadCommand(NotificationFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — succès avec 0 marquées
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithAllAlreadyRead_ShouldReturnZero()
    {
        // Arrange — toutes déjà lues
        var notifications = NotificationFixtures.CreateMixedList();
        notifications.ForEach(n => n.MarkAsRead()); // marquer toutes comme lues

        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(NotificationFixtures.ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        var command = new MarkAllAsReadCommand(NotificationFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — rien à marquer
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }
}
