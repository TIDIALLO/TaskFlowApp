using MediatR;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Shared.Contracts.Tasks;
using TaskFlow.Tasks.Application.Interfaces;
using TaskFlow.Tasks.Application.Mappings;
using TaskFlow.Tasks.Domain.Errors;

namespace TaskFlow.Tasks.Application.Queries.GetTaskById;

public sealed class GetTaskByIdQueryHandler
    : IRequestHandler<GetTaskByIdQuery, Result<TaskItemResponse>>
{
    private readonly ITaskItemRepository _taskRepository;

    public GetTaskByIdQueryHandler(ITaskItemRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<TaskItemResponse>> Handle(
        GetTaskByIdQuery request,
        CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);

        if (task is null)
            return Result<TaskItemResponse>.Failure(TaskItemErrors.NotFound);

        // Vérifier l'accès : un utilisateur ne peut voir que SES tâches
        if (task.UserId != request.UserId)
            return Result<TaskItemResponse>.Failure(TaskItemErrors.AccessDenied);

        return Result<TaskItemResponse>.Success(task.ToResponse());
    }
}
