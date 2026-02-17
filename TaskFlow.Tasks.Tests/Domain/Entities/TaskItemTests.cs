using FluentAssertions;
using TaskFlow.Tasks.Domain.Entities;
using TaskFlow.Tasks.Domain.Enums;
using TaskFlow.Tasks.Domain.Events;
using TaskFlow.Tasks.Domain.ValueObjects;
using TaskFlow.Tasks.Tests.Fixtures;

namespace TaskFlow.Tasks.Tests.Domain.Entities;

/// <summary>
/// Tests unitaires pour l'entité TaskItem.
/// 
/// ON TESTE ICI :
/// 1. La FACTORY METHOD (Create) — création avec validation
/// 2. Les TRANSITIONS DE STATUT — le cycle de vie Todo → InProgress → Done/Cancelled
/// 3. Les INVARIANTS — règles métier que l'entité protège
/// 4. Les DOMAIN EVENTS — chaque action importante lève le bon événement
/// 
/// C'est le test LE PLUS IMPORTANT du module Tasks.
/// L'entité contient la logique métier (Rich Domain Model).
/// Si ces tests passent, on est sûr que les règles du domaine sont respectées.
/// </summary>
public class TaskItemTests
{
    // ═══════════════════════════════════════════════════════
    // FACTORY METHOD — Create
    // ═══════════════════════════════════════════════════════

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var title = TaskTitle.Create(TaskFixtures.ValidTitle).Value;
        var desc = TaskDescription.Create(TaskFixtures.ValidDescription).Value;

        // Act
        var result = TaskItem.Create(title, desc, Priority.High, TaskFixtures.ValidDueDate, TaskFixtures.ValidUserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be(title);
        result.Value.Description.Should().Be(desc);
        result.Value.Priority.Should().Be(Priority.High);
        result.Value.Status.Should().Be(TaskItemStatus.Todo); // statut initial = Todo
        result.Value.UserId.Should().Be(TaskFixtures.ValidUserId);
        result.Value.Id.Should().NotBeEmpty(); // Guid généré automatiquement
        result.Value.CompletedAt.Should().BeNull(); // pas encore complétée
    }

    [Fact]
    public void Create_WithNullDueDate_ShouldReturnSuccess()
    {
        // Arrange — la date d'échéance est optionnelle
        var title = TaskTitle.Create(TaskFixtures.ValidTitle).Value;
        var desc = TaskDescription.Create(TaskFixtures.ValidDescription).Value;

        // Act
        var result = TaskItem.Create(title, desc, Priority.Low, null, TaskFixtures.ValidUserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DueDate.Should().BeNull();
    }

    [Fact]
    public void Create_WithPastDueDate_ShouldReturnFailure()
    {
        // Arrange — date dans le passé
        var title = TaskTitle.Create(TaskFixtures.ValidTitle).Value;
        var desc = TaskDescription.Create(TaskFixtures.ValidDescription).Value;

        // Act
        var result = TaskItem.Create(title, desc, Priority.Medium, TaskFixtures.PastDueDate, TaskFixtures.ValidUserId);

        // Assert — invariant violé
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskItem.DueDateInPast");
    }

    [Fact]
    public void Create_ShouldRaiseTaskCreatedEvent()
    {
        // Arrange
        var title = TaskTitle.Create(TaskFixtures.ValidTitle).Value;
        var desc = TaskDescription.Create(TaskFixtures.ValidDescription).Value;

        // Act
        var result = TaskItem.Create(title, desc, Priority.High, TaskFixtures.ValidDueDate, TaskFixtures.ValidUserId);

        // Assert — vérifier que l'événement a bien été levé
        result.Value.DomainEvents.Should().HaveCount(1);
        result.Value.DomainEvents[0].Should().BeOfType<TaskCreatedEvent>();

        var domainEvent = (TaskCreatedEvent)result.Value.DomainEvents[0];
        domainEvent.TaskId.Should().Be(result.Value.Id);
        domainEvent.Title.Should().Be(TaskFixtures.ValidTitle);
        domainEvent.UserId.Should().Be(TaskFixtures.ValidUserId);
    }

    // ═══════════════════════════════════════════════════════
    // START — Todo → InProgress
    // ═══════════════════════════════════════════════════════

    [Fact]
    public void Start_FromTodo_ShouldChangeStatusToInProgress()
    {
        // Arrange
        var task = TaskFixtures.CreateValidTask();
        task.ClearDomainEvents(); // on efface le TaskCreatedEvent pour isoler le test

        // Act
        var result = task.Start();

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(TaskItemStatus.InProgress);
    }

    [Fact]
    public void Start_FromTodo_ShouldRaiseTaskStatusChangedEvent()
    {
        // Arrange
        var task = TaskFixtures.CreateValidTask();
        task.ClearDomainEvents();

        // Act
        task.Start();

        // Assert — un événement de changement de statut doit être levé
        task.DomainEvents.Should().HaveCount(1);
        task.DomainEvents[0].Should().BeOfType<TaskStatusChangedEvent>();

        var evt = (TaskStatusChangedEvent)task.DomainEvents[0];
        evt.OldStatus.Should().Be("Todo");
        evt.NewStatus.Should().Be("InProgress");
    }

    [Fact]
    public void Start_FromInProgress_ShouldReturnFailure()
    {
        // Arrange — tâche déjà démarrée
        var task = TaskFixtures.CreateStartedTask();

        // Act
        var result = task.Start();

        // Assert — invariant : on ne peut démarrer qu'une tâche en Todo
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskItem.CannotStart");
    }

    [Fact]
    public void Start_FromDone_ShouldReturnFailure()
    {
        // Arrange — tâche terminée
        var task = TaskFixtures.CreateCompletedTask();

        // Act
        var result = task.Start();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskItem.CannotStart");
    }

    // ═══════════════════════════════════════════════════════
    // COMPLETE — InProgress → Done (ou Todo → Done)
    // ═══════════════════════════════════════════════════════

    [Fact]
    public void Complete_FromInProgress_ShouldChangeStatusToDone()
    {
        // Arrange
        var task = TaskFixtures.CreateStartedTask();
        task.ClearDomainEvents();

        // Act
        var result = task.Complete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(TaskItemStatus.Done);
        task.CompletedAt.Should().NotBeNull(); // date de complétion remplie
    }

    [Fact]
    public void Complete_ShouldRaiseBothCompletedAndStatusChangedEvents()
    {
        // Arrange
        var task = TaskFixtures.CreateStartedTask();
        task.ClearDomainEvents();

        // Act
        task.Complete();

        // Assert — DEUX événements : TaskCompleted + TaskStatusChanged
        task.DomainEvents.Should().HaveCount(2);
        task.DomainEvents.Should().ContainSingle(e => e is TaskCompletedEvent);
        task.DomainEvents.Should().ContainSingle(e => e is TaskStatusChangedEvent);
    }

    [Fact]
    public void Complete_FromDone_ShouldReturnFailure()
    {
        // Arrange — déjà terminée
        var task = TaskFixtures.CreateCompletedTask();

        // Act
        var result = task.Complete();

        // Assert — invariant : on ne peut pas terminer une tâche déjà terminée
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskItem.CannotComplete");
    }

    [Fact]
    public void Complete_FromCancelled_ShouldReturnFailure()
    {
        // Arrange — annulée
        var task = TaskFixtures.CreateCancelledTask();

        // Act
        var result = task.Complete();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskItem.CannotComplete");
    }

    // ═══════════════════════════════════════════════════════
    // CANCEL
    // ═══════════════════════════════════════════════════════

    [Fact]
    public void Cancel_FromTodo_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var task = TaskFixtures.CreateValidTask();
        task.ClearDomainEvents();

        // Act
        var result = task.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(TaskItemStatus.Cancelled);
    }

    [Fact]
    public void Cancel_FromInProgress_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var task = TaskFixtures.CreateStartedTask();
        task.ClearDomainEvents();

        // Act
        var result = task.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(TaskItemStatus.Cancelled);
    }

    [Fact]
    public void Cancel_FromDone_ShouldReturnFailure()
    {
        // Arrange — tâche terminée
        var task = TaskFixtures.CreateCompletedTask();

        // Act
        var result = task.Cancel();

        // Assert — invariant : on ne peut pas annuler une tâche terminée
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskItem.CannotCancel");
    }

    [Fact]
    public void Cancel_FromCancelled_ShouldReturnFailure()
    {
        // Arrange — déjà annulée
        var task = TaskFixtures.CreateCancelledTask();

        // Act
        var result = task.Cancel();

        // Assert — idempotence : on ne peut pas annuler deux fois
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskItem.AlreadyCancelled");
    }

    [Fact]
    public void Cancel_ShouldRaiseTaskStatusChangedEvent()
    {
        // Arrange
        var task = TaskFixtures.CreateValidTask();
        task.ClearDomainEvents();

        // Act
        task.Cancel();

        // Assert
        task.DomainEvents.Should().HaveCount(1);
        task.DomainEvents[0].Should().BeOfType<TaskStatusChangedEvent>();

        var evt = (TaskStatusChangedEvent)task.DomainEvents[0];
        evt.OldStatus.Should().Be("Todo");
        evt.NewStatus.Should().Be("Cancelled");
    }

    // ═══════════════════════════════════════════════════════
    // UPDATE METHODS — Modifications simples
    // ═══════════════════════════════════════════════════════

    [Fact]
    public void UpdateTitle_ShouldChangeTitle()
    {
        // Arrange
        var task = TaskFixtures.CreateValidTask();
        var newTitle = TaskTitle.Create("Nouveau titre").Value;

        // Act
        var result = task.UpdateTitle(newTitle);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Title.Should().Be(newTitle);
    }

    [Fact]
    public void UpdateDescription_ShouldChangeDescription()
    {
        // Arrange
        var task = TaskFixtures.CreateValidTask();
        var newDesc = TaskDescription.Create("Nouvelle description").Value;

        // Act
        var result = task.UpdateDescription(newDesc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Description.Should().Be(newDesc);
    }

    [Fact]
    public void ChangePriority_ShouldUpdatePriority()
    {
        // Arrange
        var task = TaskFixtures.CreateValidTask();

        // Act
        var result = task.ChangePriority(Priority.Critical);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Priority.Should().Be(Priority.Critical);
    }

    [Fact]
    public void ChangeDueDate_WithValidDate_ShouldUpdateDueDate()
    {
        // Arrange
        var task = TaskFixtures.CreateValidTask();
        var newDate = DateTime.UtcNow.AddDays(14);

        // Act
        var result = task.ChangeDueDate(newDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.DueDate.Should().Be(newDate);
    }

    [Fact]
    public void ChangeDueDate_WithPastDate_ShouldReturnFailure()
    {
        // Arrange
        var task = TaskFixtures.CreateValidTask();

        // Act
        var result = task.ChangeDueDate(TaskFixtures.PastDueDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskItem.DueDateInPast");
    }

    [Fact]
    public void ChangeDueDate_WithNull_ShouldRemoveDueDate()
    {
        // Arrange
        var task = TaskFixtures.CreateValidTask();

        // Act
        var result = task.ChangeDueDate(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.DueDate.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════
    // DOMAIN EVENTS — ClearDomainEvents
    // ═══════════════════════════════════════════════════════

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange — une tâche nouvellement créée a un event
        var task = TaskFixtures.CreateValidTask();
        task.DomainEvents.Should().NotBeEmpty();

        // Act
        task.ClearDomainEvents();

        // Assert — la liste est vidée (le UnitOfWork fait ça après publication)
        task.DomainEvents.Should().BeEmpty();
    }
}
