using FluentValidation;

namespace TaskFlow.Tasks.Application.Commands.CreateTask;

/// <summary>
/// Validation de surface pour CreateTaskCommand.
/// Exécuté automatiquement par le ValidationBehavior AVANT le handler.
/// Rejette les requêtes manifestement invalides sans toucher au Domain.
/// </summary>
public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    // Les priorités autorisées — en static readonly pour ne pas recréer le tableau à chaque validation
    private static readonly string[] ValidPriorities = ["Low", "Medium", "High", "Critical"];

    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description is not null); // Seulement si fournie

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required.")
            .Must(p => ValidPriorities.Contains(p))
            .WithMessage("Priority must be one of: Low, Medium, High, Critical.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
