using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Tasks.Domain.ValueObjects;

/// <summary>
/// Value Object pour la description d'une tâche.
/// La description est optionnelle (peut être vide), mais a une longueur max.
/// </summary>
public sealed record TaskDescription
{
    public const int MaxLength = 2000;

    public string Value { get; }

    private TaskDescription(string value) => Value = value;

    /// <summary>
    /// Crée une description. Si null ou vide → description vide (pas d'erreur).
    /// Seule contrainte : longueur max.
    /// </summary>
    public static Result<TaskDescription> Create(string? description)
    {
        // La description est optionnelle, on normalise null → ""
        var value = description?.Trim() ?? string.Empty;

        if (value.Length > MaxLength)
            return Result<TaskDescription>.Failure(new Error(
                "TaskDescription.TooLong",
                $"Task description must not exceed {MaxLength} characters.",
                ErrorType.Validation));

        return Result<TaskDescription>.Success(new TaskDescription(value));
    }

    /// <summary>
    /// Shortcut pour une description vide.
    /// </summary>
    public static TaskDescription Empty => new(string.Empty);
}
