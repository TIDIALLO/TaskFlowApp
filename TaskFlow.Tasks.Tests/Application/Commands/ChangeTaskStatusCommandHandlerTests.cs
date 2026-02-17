using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskFlow.Tasks.Application.Commands.ChangeTaskStatus;
using TaskFlow.Tasks.Application.Interfaces;
using TaskFlow.Tasks.Domain.Entities;
using TaskFlow.Tasks.Tests.Fixtures;

namespace TaskFlow.Tasks.Tests.Application.Commands;

/// <summary>
/// Tests unitaires pour ChangeTaskStatusCommandHandler.
/// 
/// CE QU'ON VÉRIFIE :
/// 1. Task Not Found → erreur
/// 2. Access Denied (autre utilisateur) → erreur
/// 3. Transitions valides → succès + update + save
/// 4. Transitions invalides → erreur propagée du Domain
/// 5. Statut inconnu → erreur
/// 
/// MOCK STRATEGY :
/// - Repository retourne une tâche pré-fabriquée par les Fixtures
/// - UnitOfWork simulé (pas de vraie DB)
/// - L'ENTITÉ est réelle → les invariants du Domain sont testés indirectement
/// </summary>
public class ChangeTaskStatusCommandHandlerTests
{
    private readonly Mock<ITaskItemRepository> _taskRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ChangeTaskStatusCommandHandler>> _loggerMock;
    private readonly ChangeTaskStatusCommandHandler _handler;

    public ChangeTaskStatusCommandHandlerTests()
    {
        _taskRepositoryMock = new Mock<ITaskItemRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ChangeTaskStatusCommandHandler>>();

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _handler = new ChangeTaskStatusCommandHandler(
            _taskRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithNonExistentTask_ShouldReturnNotFound()
    {
        // Arrange — le repository retourne null
        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var command = new ChangeTaskStatusCommand(Guid.NewGuid(), "InProgress", TaskFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskItem.NotFound");
    }

    [Fact]
    public async Task Handle_WithWrongUser_ShouldReturnAccessDenied()
    {
        // Arrange — la tâche appartient à ValidUserId, mais un autre user essaie
        var task = TaskFixtures.CreateValidTask();
        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var otherUserId = Guid.NewGuid(); // ← PAS le propriétaire
        var command = new ChangeTaskStatusCommand(task.Id, "InProgress", otherUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — sécurité : accès refusé
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskItem.AccessDenied");
    }

    [Fact]
    public async Task Handle_StartTask_ShouldChangeStatusToInProgress()
    {
        // Arrange — tâche en Todo
        var task = TaskFixtures.CreateValidTask();
        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var command = new ChangeTaskStatusCommand(task.Id, "InProgress", TaskFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("InProgress");

        _taskRepositoryMock.Verify(x => x.Update(task), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CompleteTask_ShouldChangeStatusToDone()
    {
        // Arrange — tâche en InProgress
        var task = TaskFixtures.CreateStartedTask();
        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var command = new ChangeTaskStatusCommand(task.Id, "Done", TaskFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Done");
        result.Value.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_CancelTask_ShouldChangeStatusToCancelled()
    {
        // Arrange — tâche en Todo
        var task = TaskFixtures.CreateValidTask();
        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var command = new ChangeTaskStatusCommand(task.Id, "Cancelled", TaskFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task Handle_StartAlreadyStartedTask_ShouldReturnFailure()
    {
        // Arrange — tâche déjà InProgress
        var task = TaskFixtures.CreateStartedTask();
        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var command = new ChangeTaskStatusCommand(task.Id, "InProgress", TaskFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — le Domain a rejeté la transition
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskItem.CannotStart");

        // Pas de save car l'opération a échoué
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CompleteAlreadyDoneTask_ShouldReturnFailure()
    {
        // Arrange — tâche déjà Done
        var task = TaskFixtures.CreateCompletedTask();
        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var command = new ChangeTaskStatusCommand(task.Id, "Done", TaskFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskItem.CannotComplete");
    }

    [Fact]
    public async Task Handle_WithInvalidStatus_ShouldReturnFailure()
    {
        // Arrange — statut qui n'existe pas
        var task = TaskFixtures.CreateValidTask();
        _taskRepositoryMock
            .Setup(x => x.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var command = new ChangeTaskStatusCommand(task.Id, "InvalidStatus", TaskFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskItem.InvalidStatus");
    }
}
