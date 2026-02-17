using MediatR;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Shared.Contracts.Tasks;

namespace TaskFlow.Tasks.Application.Commands.UpdateTask;

/// <summary>
/// Met à jour le titre, la description, la priorité et/ou la date d'échéance.
/// UserId sert à vérifier que l'utilisateur est bien le propriétaire de la tâche.
/// </summary>
public sealed record UpdateTaskCommand(
    Guid TaskId,
    string Title,
    string? Description,
    string Priority,
    DateTime? DueDate,
    Guid UserId) : IRequest<Result<TaskItemResponse>>;
