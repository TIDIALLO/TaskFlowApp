using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Shared.Contracts.Tasks;
using TaskFlow.Tasks.Application.Interfaces;
using TaskFlow.Tasks.Application.Mappings;
using TaskFlow.Tasks.Domain.Enums;
using TaskFlow.Tasks.Domain.Errors;
using TaskFlow.Tasks.Domain.ValueObjects;

namespace TaskFlow.Tasks.Application.Commands.UpdateTask;

public sealed class UpdateTaskCommandHandler
    : IRequestHandler<UpdateTaskCommand, Result<TaskItemResponse>>
{
    private readonly ITaskItemRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateTaskCommandHandler> _logger;

    public UpdateTaskCommandHandler(
        ITaskItemRepository taskRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateTaskCommandHandler> logger)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TaskItemResponse>> Handle(
        UpdateTaskCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Récupérer la tâche
        var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
            return Result<TaskItemResponse>.Failure(TaskItemErrors.NotFound);

        // 2. Vérifier que l'utilisateur est le propriétaire
        //    SÉCURITÉ : empêche un user de modifier les tâches d'un autre
        if (task.UserId != request.UserId)
            return Result<TaskItemResponse>.Failure(TaskItemErrors.AccessDenied);

        // 3. Créer les Value Objects (validation Domain)
        var titleResult = TaskTitle.Create(request.Title);
        if (titleResult.IsFailure)
            return Result<TaskItemResponse>.Failure(titleResult.Error);

        var descriptionResult = TaskDescription.Create(request.Description);
        if (descriptionResult.IsFailure)
            return Result<TaskItemResponse>.Failure(descriptionResult.Error);

        if (!Enum.TryParse<Priority>(request.Priority, ignoreCase: true, out var priority))
            return Result<TaskItemResponse>.Failure(new Error(
                "TaskItem.InvalidPriority", "Invalid priority value.", ErrorType.Validation));

        // 4. Appliquer les modifications via les méthodes métier de l'entité
        //    On n'accède JAMAIS directement aux propriétés (pas de task.Title = ...)
        task.UpdateTitle(titleResult.Value);
        task.UpdateDescription(descriptionResult.Value);
        task.ChangePriority(priority);

        var dueDateResult = task.ChangeDueDate(request.DueDate);
        if (dueDateResult.IsFailure)
            return Result<TaskItemResponse>.Failure(dueDateResult.Error);

        // 5. Persister
        _taskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Task {TaskId} updated", task.Id);

        return Result<TaskItemResponse>.Success(task.ToResponse());
    }
}
