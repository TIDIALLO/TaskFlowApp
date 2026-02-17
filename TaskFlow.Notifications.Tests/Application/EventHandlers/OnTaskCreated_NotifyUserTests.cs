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
/// Tests pour OnTaskCreated_NotifyUser.
/// 
/// PATTERN TESTÉ : Domain Event → Cross-Module Reaction
/// 
/// FLUX DANS L'APPLICATION RÉELLE :
/// 1. TaskItem.Create() → Raise(TaskCreatedEvent)
/// 2. UnitOfWork.SaveChanges → _mediator.Publish(TaskCreatedEvent)
/// 3. OnTaskCreated_NotifyUser.Handle() → crée une Notification
/// 
/// ICI ON TESTE UNIQUEMENT l'étape 3 de façon isolée.
/// L'étape 1 est testée dans TaskItemTests.
/// L'étape 2 (UnitOfWork dispatch) pourrait être un test d'intégration.
/// </summary>
public class OnTaskCreated_NotifyUserTests
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly Mock<INotificationUnitOfWork> _unitOfWorkMock;
    private readonly OnTaskCreated_NotifyUser _handler;

    public OnTaskCreated_NotifyUserTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _unitOfWorkMock = new Mock<INotificationUnitOfWork>();

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _handler = new OnTaskCreated_NotifyUser(
            _repositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateTaskCreatedNotification()
    {
        // Arrange — simuler un Domain Event du module Tasks
        var taskId = Guid.NewGuid();
        var domainEvent = new TaskCreatedEvent(taskId, "Corriger le bug #42", "High", NotificationFixtures.ValidUserId);

        Notification? capturedNotification = null;
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => capturedNotification = n)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert — une notification a été créée et persistée
        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Vérifier le contenu
        capturedNotification.Should().NotBeNull();
        capturedNotification!.UserId.Should().Be(NotificationFixtures.ValidUserId);
        capturedNotification.Type.Should().Be(NotificationType.TaskCreated);
        capturedNotification.Title.Should().Contain("tâche");
        capturedNotification.Message.Should().Contain("Corriger le bug #42");
        capturedNotification.IsRead.Should().BeFalse();
    }
}
