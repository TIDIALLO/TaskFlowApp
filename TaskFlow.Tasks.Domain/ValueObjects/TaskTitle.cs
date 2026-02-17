using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Tasks.Domain.ValueObjects;

/// <summary>
/// Value Object pour le titre d'une tâche.
/// 
/// RAPPEL Value Object :
/// - Immutable (pas de setter public)
/// - Comparé par valeur (grâce au record)
/// - Se valide lui-même dans la factory method Create()
/// - Le constructeur est privé → on FORCE le passage par Create()
/// </summary>
public sealed record TaskTitle
{
    public const int MaxLength = 200;

    public string Value { get; }

    private TaskTitle(string value) => Value = value;

    public static Result<TaskTitle> Create(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result<TaskTitle>.Failure(new Error(
                "TaskTitle.Empty", "Task title is required.", ErrorType.Validation));

        if (title.Length > MaxLength)
            return Result<TaskTitle>.Failure(new Error(
                "TaskTitle.TooLong", $"Task title must not exceed {MaxLength} characters.", ErrorType.Validation));

        return Result<TaskTitle>.Success(new TaskTitle(title.Trim()));
    }
}
