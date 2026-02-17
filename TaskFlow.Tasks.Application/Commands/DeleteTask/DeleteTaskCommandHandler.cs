using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Tasks.Application.Interfaces;
using TaskFlow.Tasks.Domain.Errors;

namespace TaskFlow.Tasks.Application.Commands.DeleteTask;

public sealed class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result>
{
    private readonly ITaskItemRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteTaskCommandHandler> _logger;

    public DeleteTaskCommandHandler(
        ITaskItemRepository taskRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteTaskCommandHandler> logger)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task is null)
            return Result.Failure(TaskItemErrors.NotFound);

        if (task.UserId != request.UserId)
            return Result.Failure(TaskItemErrors.AccessDenied);

        _taskRepository.Remove(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Task {TaskId} deleted by user {UserId}", request.TaskId, request.UserId);

        return Result.Success();
    }
}
