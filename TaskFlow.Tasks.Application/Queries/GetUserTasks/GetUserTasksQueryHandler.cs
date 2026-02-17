using MediatR;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Shared.Contracts.Tasks;
using TaskFlow.Tasks.Application.Interfaces;
using TaskFlow.Tasks.Application.Mappings;

namespace TaskFlow.Tasks.Application.Queries.GetUserTasks;

public sealed class GetUserTasksQueryHandler
    : IRequestHandler<GetUserTasksQuery, Result<List<TaskItemResponse>>>
{
    private readonly ITaskItemRepository _taskRepository;

    public GetUserTasksQueryHandler(ITaskItemRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<List<TaskItemResponse>>> Handle(
        GetUserTasksQuery request,
        CancellationToken cancellationToken)
    {
        // Récupère uniquement les tâches de l'utilisateur connecté
        // Pas besoin de vérification d'accès : le filtre par UserId est déjà la protection
        var tasks = await _taskRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        return Result<List<TaskItemResponse>>.Success(tasks.ToResponseList());
    }
}
