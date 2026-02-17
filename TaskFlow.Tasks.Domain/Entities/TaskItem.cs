using TaskFlow.Shared.Kernel.Primitives;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Tasks.Domain.Enums;
using TaskFlow.Tasks.Domain.Errors;
using TaskFlow.Tasks.Domain.Events;
using TaskFlow.Tasks.Domain.ValueObjects;

namespace TaskFlow.Tasks.Domain.Entities;

/// <summary>
/// Entité principale du module Tasks.
/// 
/// PRINCIPES DDD appliqués ici :
/// 1. ENCAPSULATION : tous les setters sont "private set"
/// 2. RICH DOMAIN MODEL : l'entité contient les RÈGLES MÉTIER
/// 3. FACTORY METHOD (Create) : le constructeur est privé
/// 4. INVARIANTS : protégés par les méthodes métier
/// 5. DOMAIN EVENTS : chaque action importante lève un événement (Raise)
/// </summary>
public sealed class TaskItem : Entity
{
    public TaskTitle Title { get; private set; }
    public TaskDescription Description { get; private set; }
    public Priority Priority { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateTime? DueDate { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private TaskItem(
        Guid id,
        TaskTitle title,
        TaskDescription description,
        Priority priority,
        DateTime? dueDate,
        Guid userId) : base(id)
    {
        Title = title;
        Description = description;
        Priority = priority;
        Status = TaskItemStatus.Todo;
        DueDate = dueDate;
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }

#pragma warning disable CS8618
    private TaskItem() { }
#pragma warning restore CS8618

    /// <summary>
    /// Factory method : crée un nouveau TaskItem et lève TaskCreatedEvent.
    /// </summary>
    public static Result<TaskItem> Create(
        TaskTitle title,
        TaskDescription description,
        Priority priority,
        DateTime? dueDate,
        Guid userId)
    {
        if (dueDate.HasValue && dueDate.Value.Date < DateTime.UtcNow.Date)
            return Result<TaskItem>.Failure(TaskItemErrors.DueDateInPast);

        var task = new TaskItem(Guid.NewGuid(), title, description, priority, dueDate, userId);

        // NOUVEAU : lever un Domain Event
        // L'entité dit "je viens d'être créée" — elle ne sait PAS qui écoute
        task.Raise(new TaskCreatedEvent(
            task.Id,
            title.Value,
            priority.ToString(),
            userId));

        return Result<TaskItem>.Success(task);
    }

    // ══════════════════════════════════════════════════════════════
    // MÉTHODES MÉTIER — Chacune protège un invariant + lève un event
    // ══════════════════════════════════════════════════════════════

    public Result Start()
    {
        if (Status != TaskItemStatus.Todo)
            return Result.Failure(TaskItemErrors.CannotStart);

        var oldStatus = Status.ToString();
        Status = TaskItemStatus.InProgress;

        Raise(new TaskStatusChangedEvent(Id, Title.Value, oldStatus, Status.ToString(), UserId));

        return Result.Success();
    }

    public Result Complete()
    {
        if (Status is TaskItemStatus.Done or TaskItemStatus.Cancelled)
            return Result.Failure(TaskItemErrors.CannotComplete);

        var oldStatus = Status.ToString();
        Status = TaskItemStatus.Done;
        CompletedAt = DateTime.UtcNow;

        // Lever DEUX events : un spécifique (TaskCompleted) + un générique (StatusChanged)
        Raise(new TaskCompletedEvent(Id, Title.Value, UserId));
        Raise(new TaskStatusChangedEvent(Id, Title.Value, oldStatus, Status.ToString(), UserId));

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == TaskItemStatus.Done)
            return Result.Failure(TaskItemErrors.CannotCancel);

        if (Status == TaskItemStatus.Cancelled)
            return Result.Failure(TaskItemErrors.AlreadyCancelled);

        var oldStatus = Status.ToString();
        Status = TaskItemStatus.Cancelled;

        Raise(new TaskStatusChangedEvent(Id, Title.Value, oldStatus, Status.ToString(), UserId));

        return Result.Success();
    }

    public Result UpdateTitle(TaskTitle newTitle)
    {
        Title = newTitle;
        return Result.Success();
    }

    public Result UpdateDescription(TaskDescription newDescription)
    {
        Description = newDescription;
        return Result.Success();
    }

    public Result ChangePriority(Priority newPriority)
    {
        Priority = newPriority;
        return Result.Success();
    }

    public Result ChangeDueDate(DateTime? newDueDate)
    {
        if (newDueDate.HasValue && newDueDate.Value.Date < DateTime.UtcNow.Date)
            return Result.Failure(TaskItemErrors.DueDateInPast);

        DueDate = newDueDate;
        return Result.Success();
    }
}
