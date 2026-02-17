using MediatR;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Shared.Contracts.Tasks;

namespace TaskFlow.Tasks.Application.Queries.GetTaskById;

/// <summary>
/// Query = intention de LIRE des données SANS modifier l'état du système.
/// C'est le "Q" de CQRS.
/// 
/// Les Queries sont toujours des records immutables avec les paramètres de filtrage.
/// </summary>
public sealed record GetTaskByIdQuery(Guid TaskId, Guid UserId) : IRequest<Result<TaskItemResponse>>;
