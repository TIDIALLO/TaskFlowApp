using MediatR;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Shared.Contracts.Tasks;

namespace TaskFlow.Tasks.Application.Commands.CreateTask;

/// <summary>
/// Command = intention de MODIFIER l'état du système (créer, mettre à jour, supprimer).
/// C'est le "C" de CQRS (Command Query Responsibility Segregation).
/// 
/// IRequest&lt;Result&lt;TaskItemResponse&gt;&gt; signifie :
/// - C'est une requête MediatR
/// - Elle retourne un Result&lt;TaskItemResponse&gt; (succès avec un DTO, ou échec avec une erreur)
/// 
/// record = immutable + comparaison par valeur + ToString() gratuit.
/// Parfait pour les messages (commands/queries) qui ne changent jamais après création.
/// </summary>
public sealed record CreateTaskCommand(
    string Title,
    string? Description,
    string Priority,        // "Low", "Medium", "High", "Critical" — reçu en string du client
    DateTime? DueDate,
    Guid UserId) : IRequest<Result<TaskItemResponse>>;
