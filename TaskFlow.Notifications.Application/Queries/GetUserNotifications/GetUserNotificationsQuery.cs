using MediatR;
using TaskFlow.Shared.Contracts.Notifications;

namespace TaskFlow.Notifications.Application.Queries.GetUserNotifications;

/// <summary>
/// QUERY (CQRS côté lecture) : récupérer les notifications d'un utilisateur.
/// 
/// IRequest<List<NotificationResponse>> : "cette requête attend une liste de NotificationResponse"
/// Le Handler correspondant implémentera IRequestHandler<Query, Response>.
/// </summary>
public sealed record GetUserNotificationsQuery(Guid UserId) : IRequest<List<NotificationResponse>>;
