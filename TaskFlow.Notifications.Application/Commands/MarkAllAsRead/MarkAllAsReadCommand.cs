using MediatR;
using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Notifications.Application.Commands.MarkAllAsRead;

/// <summary>
/// Command : marquer TOUTES les notifications d'un utilisateur comme lues.
/// Retourne Result<int> : le nombre de notifications marquées comme lues.
/// </summary>
public sealed record MarkAllAsReadCommand(Guid UserId) : IRequest<Result<int>>;
