using FluentAssertions;
using Moq;
using TaskFlow.Notifications.Application.Commands.MarkAsRead;
using TaskFlow.Notifications.Application.Interfaces;
using TaskFlow.Notifications.Domain.Entities;
using TaskFlow.Notifications.Tests.Fixtures;

namespace TaskFlow.Notifications.Tests.Application.Commands;

/// <summary>
/// Tests pour MarkAsReadCommandHandler.
/// 
/// SCÉNARIOS TESTÉS :
/// 1. Notification trouvée + bon user → succès, MarkAsRead appelé, SaveChanges
/// 2. Notification introuvable → erreur NotFound
/// 3. Notification d'un autre user → erreur Forbidden (sécurité)
/// 4. Notification déjà lue → succès quand même (idempotent)
/// </summary>
public class MarkAsReadCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly Mock<INotificationUnitOfWork> _unitOfWorkMock;
    private readonly MarkAsReadCommandHandler _handler;

    public MarkAsReadCommandHandlerTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _unitOfWorkMock = new Mock<INotificationUnitOfWork>();

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _handler = new MarkAsReadCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidNotification_ShouldMarkAsReadAndSave()
    {
        // Arrange — notification non lue qui appartient à l'utilisateur
        var notification = NotificationFixtures.CreateUnread();
        _repositoryMock
            .Setup(x => x.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var command = new MarkAsReadCommand(notification.Id, NotificationFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue(); // la notification est maintenant lue
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentNotification_ShouldReturnNotFound()
    {
        // Arrange — le repository retourne null
        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        var command = new MarkAsReadCommand(Guid.NewGuid(), NotificationFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.NotFound");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithWrongUser_ShouldReturnForbidden()
    {
        // Arrange — notification appartient à ValidUserId, mais OtherUserId essaie
        var notification = NotificationFixtures.CreateUnread();
        _repositoryMock
            .Setup(x => x.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var command = new MarkAsReadCommand(notification.Id, NotificationFixtures.OtherUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — SÉCURITÉ : un utilisateur ne peut pas lire les notifications d'un autre
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.Forbidden");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAlreadyReadNotification_ShouldStillSucceed()
    {
        // Arrange — notification déjà lue
        var notification = NotificationFixtures.CreateRead();
        _repositoryMock
            .Setup(x => x.GetByIdAsync(notification.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var command = new MarkAsReadCommand(notification.Id, NotificationFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — idempotent : succès même si déjà lue
        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
    }
}
