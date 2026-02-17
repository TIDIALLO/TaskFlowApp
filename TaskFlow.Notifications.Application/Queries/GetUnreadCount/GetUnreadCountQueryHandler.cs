using MediatR;
using TaskFlow.Notifications.Application.Interfaces;
using TaskFlow.Shared.Contracts.Notifications;

namespace TaskFlow.Notifications.Application.Queries.GetUnreadCount;

public sealed class GetUnreadCountQueryHandler
    : IRequestHandler<GetUnreadCountQuery, UnreadCountResponse>
{
    private readonly INotificationRepository _repository;

    public GetUnreadCountQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<UnreadCountResponse> Handle(
        GetUnreadCountQuery request,
        CancellationToken cancellationToken)
    {
        var count = await _repository.GetUnreadCountAsync(request.UserId, cancellationToken);
        return new UnreadCountResponse(count);
    }
}
