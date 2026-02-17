using MediatR;
using TaskFlow.Notifications.Application.Interfaces;
using TaskFlow.Notifications.Application.Mappings;
using TaskFlow.Shared.Contracts.Notifications;

namespace TaskFlow.Notifications.Application.Queries.GetUserNotifications;

/// <summary>
/// Handler pour GetUserNotificationsQuery.
/// Très simple : appelle le repository et mappe en DTOs.
/// 
/// C'est la BEAUTÉ de CQRS : les queries sont ultra-simples,
/// toute la complexité est dans les Commands.
/// </summary>
public sealed class GetUserNotificationsQueryHandler
    : IRequestHandler<GetUserNotificationsQuery, List<NotificationResponse>>
{
    private readonly INotificationRepository _repository;

    public GetUserNotificationsQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<NotificationResponse>> Handle(
        GetUserNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var notifications = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        return notifications.ToResponseList();
    }
}
