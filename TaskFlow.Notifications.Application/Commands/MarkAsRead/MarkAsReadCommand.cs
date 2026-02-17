using MediatR;
using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Notifications.Application.Commands.MarkAsRead;

/// <summary>
/// Command : marquer UNE notification comme lue.
/// Retourne Result (pas Result<T>) car pas de données à renvoyer, juste succès/échec.
/// </summary>
public sealed record MarkAsReadCommand(Guid NotificationId, Guid UserId) : IRequest<Result>;
