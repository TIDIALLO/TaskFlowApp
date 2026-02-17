using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Tasks.Domain.Errors;

/// <summary>
/// Toutes les erreurs métier du module Tasks.
/// Centralisées ici pour :
/// 1. Éviter les strings magiques éparpillées dans le code
/// 2. Réutiliser la même erreur partout (DRY)
/// 3. Faciliter les tests (on compare avec TaskItemErrors.NotFound)
/// </summary>
public static class TaskItemErrors
{
    public static readonly Error NotFound = new(
        "TaskItem.NotFound", "Task was not found.", ErrorType.NotFound);

    public static readonly Error DueDateInPast = new(
        "TaskItem.DueDateInPast", "Due date cannot be in the past.", ErrorType.Validation);

    public static readonly Error CannotStart = new(
        "TaskItem.CannotStart", "Only tasks with 'Todo' status can be started.", ErrorType.Validation);

    public static readonly Error CannotComplete = new(
        "TaskItem.CannotComplete", "Cannot complete a task that is already done or cancelled.", ErrorType.Validation);

    public static readonly Error CannotCancel = new(
        "TaskItem.CannotCancel", "Cannot cancel a completed task.", ErrorType.Validation);

    public static readonly Error AlreadyCancelled = new(
        "TaskItem.AlreadyCancelled", "Task is already cancelled.", ErrorType.Validation);

    public static readonly Error AccessDenied = new(
        "TaskItem.AccessDenied", "You do not have access to this task.", ErrorType.Forbidden);
}
