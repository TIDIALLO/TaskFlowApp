using FluentAssertions;
using Moq;
using TaskFlow.Notifications.Application.EventHandlers;
using TaskFlow.Notifications.Application.Interfaces;
using TaskFlow.Notifications.Domain.Entities;
using TaskFlow.Notifications.Domain.Enums;
using TaskFlow.Notifications.Tests.Fixtures;
using TaskFlow.Users.Application.Notifications;

namespace TaskFlow.Notifications.Tests.Application.EventHandlers;

/// <summary>
/// Tests pour OnUserRegistered_CreateWelcomeNotification.
/// 
/// C'EST LE TEST LE PLUS IMPORTANT pour le pattern CROSS-MODULE.
/// 
/// WHAT WE VERIFY :
/// Quand le module Users publie UserRegisteredNotification,
/// le module Notifications crée automatiquement une notification de bienvenue.
/// 
/// Ce test prouve que :
/// 1. Le handler reçoit correctement l'événement d'un autre module
/// 2. Il crée une Notification de type Welcome
/// 3. Il la persiste via son propre Repository + UnitOfWork
/// 
/// Les modules restent DÉCOUPLÉS : Users ne sait pas que Notifications existe.
/// </summary>
public class OnUserRegistered_CreateWelcomeNotificationTests
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly Mock<INotificationUnitOfWork> _unitOfWorkMock;
    private readonly OnUserRegistered_CreateWelcomeNotification _handler;

    public OnUserRegistered_CreateWelcomeNotificationTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _unitOfWorkMock = new Mock<INotificationUnitOfWork>();

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _handler = new OnUserRegistered_CreateWelcomeNotification(
            _repositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateWelcomeNotification()
    {
        // Arrange — simuler l'événement venant du module Users
        var notification = new UserRegisteredNotification(
            NotificationFixtures.ValidUserId,
            "test@example.com",
            "Jean Dupont");

        Notification? capturedNotification = null;
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => capturedNotification = n)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert — une notification de bienvenue a été créée
        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Vérifier le contenu de la notification capturée
        capturedNotification.Should().NotBeNull();
        capturedNotification!.UserId.Should().Be(NotificationFixtures.ValidUserId);
        capturedNotification.Type.Should().Be(NotificationType.Welcome);
        capturedNotification.Title.Should().Contain("Bienvenue");
        capturedNotification.Message.Should().Contain("Jean Dupont");
        capturedNotification.IsRead.Should().BeFalse();
    }
}
