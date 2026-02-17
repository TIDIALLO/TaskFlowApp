using FluentValidation;

namespace TaskFlow.Tasks.Application.Commands.ChangeTaskStatus;

public sealed class ChangeTaskStatusCommandValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    private static readonly string[] ValidTransitions = ["InProgress", "Done", "Cancelled"];

    public ChangeTaskStatusCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty().WithMessage("Task ID is required.");

        RuleFor(x => x.NewStatus)
            .NotEmpty().WithMessage("New status is required.")
            .Must(s => ValidTransitions.Contains(s))
            .WithMessage("Status must be one of: InProgress, Done, Cancelled.");

        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
    }
}
