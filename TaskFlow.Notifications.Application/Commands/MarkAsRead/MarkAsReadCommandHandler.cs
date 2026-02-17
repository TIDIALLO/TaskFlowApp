using MediatR;
using TaskFlow.Notifications.Application.Interfaces;
using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Notifications.Application.Commands.MarkAsRead;

public sealed class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Result>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationUnitOfWork _unitOfWork;

    public MarkAsReadCommandHandler(
        INotificationRepository repository,
        INotificationUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _repository.GetByIdAsync(request.NotificationId, cancellationToken);

        if (notification is null)
            return Result.Failure(new Error(
                "Notification.NotFound",
                "Notification introuvable.",
                ErrorType.NotFound));

        // Sécurité : vérifier que la notification appartient bien à l'utilisateur
        if (notification.UserId != request.UserId)
            return Result.Failure(new Error(
                "Notification.Forbidden",
                "Cette notification ne vous appartient pas.",
                ErrorType.Forbidden));

        notification.MarkAsRead();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
