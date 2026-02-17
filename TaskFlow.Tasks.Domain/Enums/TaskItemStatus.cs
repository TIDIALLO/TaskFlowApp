namespace TaskFlow.Tasks.Domain.Enums;

/// <summary>
/// Cycle de vie d'une tâche :
/// Todo → InProgress → Done
///                   → Cancelled (à tout moment)
/// 
/// POURQUOI "TaskItemStatus" et pas "TaskStatus" ?
/// Parce que System.Threading.Tasks.TaskStatus existe déjà dans .NET !
/// Si on met "TaskStatus", le compilateur sera confus. Même raison
/// pour laquelle notre entité s'appelle "TaskItem" et pas "Task".
/// </summary>
public enum TaskItemStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2,
    Cancelled = 3
}
