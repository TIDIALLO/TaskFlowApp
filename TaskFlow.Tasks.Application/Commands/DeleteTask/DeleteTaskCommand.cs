using MediatR;
using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Tasks.Application.Commands.DeleteTask;

/// <summary>
/// Supprime une tâche. Retourne Result (sans valeur) car on ne retourne rien après une suppression.
/// Le controller retournera 204 No Content (via HandleResult dans ApiController).
/// </summary>
public sealed record DeleteTaskCommand(
    Guid TaskId,
    Guid UserId) : IRequest<Result>;
