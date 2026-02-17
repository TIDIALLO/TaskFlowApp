using MediatR;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Shared.Contracts.Tasks;

namespace TaskFlow.Tasks.Application.Commands.ChangeTaskStatus;

/// <summary>
/// Change le statut d'une tâche (Start, Complete, Cancel).
/// 
/// POURQUOI une commande séparée pour le statut ?
/// Le changement de statut a des RÈGLES MÉTIER spécifiques 
/// (ex: on ne peut pas démarrer une tâche terminée).
/// C'est une opération distincte de la mise à jour des champs "data" (titre, description...).
/// Séparer les commandes = Single Responsibility.
/// </summary>
public sealed record ChangeTaskStatusCommand(
    Guid TaskId,
    string NewStatus,    // "InProgress", "Done", "Cancelled"
    Guid UserId) : IRequest<Result<TaskItemResponse>>;
