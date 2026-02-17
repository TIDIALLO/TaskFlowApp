using MediatR;
using TaskFlow.Shared.Contracts.Notifications;

namespace TaskFlow.Notifications.Application.Queries.GetUnreadCount;

/// <summary>
/// Query pour obtenir le nombre de notifications non lues.
/// Appelée en polling par le NavMenu (toutes les 30 secondes par ex).
/// </summary>
public sealed record GetUnreadCountQuery(Guid UserId) : IRequest<UnreadCountResponse>;
