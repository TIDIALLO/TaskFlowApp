using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Shared.Contracts.Tasks;
using TaskFlow.Tasks.Application.Interfaces;
using TaskFlow.Tasks.Application.Mappings;
using TaskFlow.Tasks.Domain.Enums;
using TaskFlow.Tasks.Domain.Errors;

namespace TaskFlow.Tasks.Application.Commands.ChangeTaskStatus;

public sealed class ChangeTaskStatusCommandHandler
    : IRequestHandler<ChangeTaskStatusCommand, Result<TaskItemResponse>>
{
    private readonly ITaskItemRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChangeTaskStatusCommandHandler> _logger;

    public ChangeTaskStatusCommandHandler(
        ITaskItemRepository taskRepository,
        IUnitOfWork unitOfWork,
        ILogger<ChangeTaskStatusCommandHandler> logger)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TaskItemResponse>> Handle(
        ChangeTaskStatusCommand request,
        CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
            return Result<TaskItemResponse>.Failure(TaskItemErrors.NotFound);

        if (task.UserId != request.UserId)
            return Result<TaskItemResponse>.Failure(TaskItemErrors.AccessDenied);

        // Appliquer la transition de statut via les méthodes métier de l'entité.
        // L'ENTITÉ protège ses invariants : si la transition est invalide, elle retourne Failure.
        // Le handler ne fait QUE orchestrer — il ne décide pas des règles.
        var result = request.NewStatus switch
        {
            "InProgress" => task.Start(),
            "Done" => task.Complete(),
            "Cancelled" => task.Cancel(),
            _ => Result.Failure(new Error(
                "TaskItem.InvalidStatus", "Invalid status transition.", ErrorType.Validation))
        };

        if (result.IsFailure)
            return Result<TaskItemResponse>.Failure(result.Error);

        _taskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Task {TaskId} status changed to {Status}", task.Id, request.NewStatus);

        return Result<TaskItemResponse>.Success(task.ToResponse());
    }
}
