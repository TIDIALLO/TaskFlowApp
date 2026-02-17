using MediatR;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Shared.Contracts.Tasks;

namespace TaskFlow.Tasks.Application.Queries.GetUserTasks;

/// <summary>
/// Récupère toutes les tâches d'un utilisateur.
/// </summary>
public sealed record GetUserTasksQuery(Guid UserId) : IRequest<Result<List<TaskItemResponse>>>;
