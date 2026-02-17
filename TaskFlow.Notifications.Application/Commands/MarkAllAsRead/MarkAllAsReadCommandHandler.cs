using MediatR;
using TaskFlow.Notifications.Application.Interfaces;
using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Notifications.Application.Commands.MarkAllAsRead;

public sealed class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, Result<int>>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationUnitOfWork _unitOfWork;

    public MarkAllAsReadCommandHandler(
        INotificationRepository repository,
        INotificationUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var notifications = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        var unread = notifications.Where(n => !n.IsRead).ToList();

        foreach (var notification in unread)
        {
            notification.MarkAsRead();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(unread.Count);
    }
}
