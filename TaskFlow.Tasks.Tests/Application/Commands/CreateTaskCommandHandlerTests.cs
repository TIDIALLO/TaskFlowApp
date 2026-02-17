using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Tasks.Application.Commands.CreateTask;
using TaskFlow.Tasks.Application.Interfaces;
using TaskFlow.Tasks.Domain.Entities;
using TaskFlow.Tasks.Tests.Fixtures;

namespace TaskFlow.Tasks.Tests.Application.Commands;

/// <summary>
/// Tests unitaires pour CreateTaskCommandHandler.
/// 
/// ON TESTE LE HANDLER, PAS LE DOMAIN :
/// Le handler est un orchestrateur. Il fait :
/// 1. Crée les Value Objects
/// 2. Appelle la Factory Method de l'entité
/// 3. Persiste via Repository + UnitOfWork
/// 
/// On MOCK les dépendances (Repository, UnitOfWork) pour isoler le handler.
/// Le Domain (TaskTitle, TaskItem) est RÉEL — on ne le mock pas.
/// C'est la bonne pratique : mocker les I/O, garder la logique réelle.
/// </summary>
public class CreateTaskCommandHandlerTests
{
    private readonly Mock<ITaskItemRepository> _taskRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateTaskCommandHandler>> _loggerMock;
    private readonly CreateTaskCommandHandler _handler;

    public CreateTaskCommandHandlerTests()
    {
        _taskRepositoryMock = new Mock<ITaskItemRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateTaskCommandHandler>>();

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _handler = new CreateTaskCommandHandler(
            _taskRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessWithTask()
    {
        // Arrange
        var command = new CreateTaskCommand(
            TaskFixtures.ValidTitle,
            TaskFixtures.ValidDescription,
            TaskFixtures.ValidPriority,
            TaskFixtures.ValidDueDate,
            TaskFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be(TaskFixtures.ValidTitle);
        result.Value.Priority.Should().Be(TaskFixtures.ValidPriority);
        result.Value.Status.Should().Be("Todo");

        // Vérifie que le repository a bien été appelé
        _taskRepositoryMock.Verify(x => x.Add(It.IsAny<TaskItem>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyTitle_ShouldReturnFailure()
    {
        // Arrange — titre vide → erreur du Value Object TaskTitle
        var command = new CreateTaskCommand(
            "",
            TaskFixtures.ValidDescription,
            TaskFixtures.ValidPriority,
            TaskFixtures.ValidDueDate,
            TaskFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — le handler propage l'erreur du Domain
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskTitle.Empty");

        // Le repository ne doit PAS être appelé (early return)
        _taskRepositoryMock.Verify(x => x.Add(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidPriority_ShouldReturnFailure()
    {
        // Arrange — priorité inexistante
        var command = new CreateTaskCommand(
            TaskFixtures.ValidTitle,
            TaskFixtures.ValidDescription,
            "Urgentissime",  // ← n'existe pas dans l'enum Priority
            TaskFixtures.ValidDueDate,
            TaskFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskItem.InvalidPriority");

        _taskRepositoryMock.Verify(x => x.Add(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithPastDueDate_ShouldReturnFailure()
    {
        // Arrange — date dans le passé → rejeté par TaskItem.Create
        var command = new CreateTaskCommand(
            TaskFixtures.ValidTitle,
            TaskFixtures.ValidDescription,
            TaskFixtures.ValidPriority,
            TaskFixtures.PastDueDate,
            TaskFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskItem.DueDateInPast");
    }

    [Fact]
    public async Task Handle_WithNullDescription_ShouldReturnSuccess()
    {
        // Arrange — description optionnelle
        var command = new CreateTaskCommand(
            TaskFixtures.ValidTitle,
            null,
            TaskFixtures.ValidPriority,
            TaskFixtures.ValidDueDate,
            TaskFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().BeEmpty(); // null → ""
    }

    [Fact]
    public async Task Handle_WithNullDueDate_ShouldReturnSuccess()
    {
        // Arrange — date optionnelle
        var command = new CreateTaskCommand(
            TaskFixtures.ValidTitle,
            TaskFixtures.ValidDescription,
            TaskFixtures.ValidPriority,
            null,
            TaskFixtures.ValidUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DueDate.Should().BeNull();
    }
}
