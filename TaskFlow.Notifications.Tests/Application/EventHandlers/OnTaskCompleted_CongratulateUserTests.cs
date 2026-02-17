using FluentAssertions;
using Moq;
using TaskFlow.Notifications.Application.EventHandlers;
using TaskFlow.Notifications.Application.Interfaces;
using TaskFlow.Notifications.Domain.Entities;
using TaskFlow.Notifications.Domain.Enums;
using TaskFlow.Notifications.Tests.Fixtures;
using TaskFlow.Tasks.Domain.Events;

namespace TaskFlow.Notifications.Tests.Application.EventHandlers;

/// <summary>
/// Tests pour OnTaskCompleted_CongratulateUser.
/// 
/// PATTERN TESTÉ : Pub/Sub — un événement, plusieurs handlers possibles
/// 
/// TaskCompletedEvent est publié par le UnitOfWork du module Tasks.
/// Ce handler dans le module Notifications le capte et crée une
/// notification de félicitations.
/// 
/// DANS UN VRAI PROJET, d'autres handlers pourraient aussi écouter cet event :
/// - Module Analytics → incrémente un compteur
/// - Module Email → envoie un email de résumé
/// C'est la puissance du Pub/Sub : 1 publisher, N subscribers.
/// </summary>
public class OnTaskCompleted_CongratulateUserTests
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly Mock<INotificationUnitOfWork> _unitOfWorkMock;
    private readonly OnTaskCompleted_CongratulateUser _handler;

    public OnTaskCompleted_CongratulateUserTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _unitOfWorkMock = new Mock<INotificationUnitOfWork>();

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _handler = new OnTaskCompleted_CongratulateUser(
            _repositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateCompletedNotification()
    {
        // Arrange — simuler un Domain Event de complétion
        var taskId = Guid.NewGuid();
        var domainEvent = new TaskCompletedEvent(taskId, "Déployer en production", NotificationFixtures.ValidUserId);

        Notification? capturedNotification = null;
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => capturedNotification = n)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert — notification de félicitations créée et persistée
        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Vérifier le contenu
        capturedNotification.Should().NotBeNull();
        capturedNotification!.UserId.Should().Be(NotificationFixtures.ValidUserId);
        capturedNotification.Type.Should().Be(NotificationType.TaskCompleted);
        capturedNotification.Title.Should().Contain("terminée");
        capturedNotification.Message.Should().Contain("Déployer en production");
        capturedNotification.IsRead.Should().BeFalse();
    }
}
