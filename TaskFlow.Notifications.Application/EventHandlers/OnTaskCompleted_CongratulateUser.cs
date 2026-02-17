using MediatR;
using TaskFlow.Notifications.Application.Interfaces;
using TaskFlow.Notifications.Domain.Entities;
using TaskFlow.Notifications.Domain.Enums;
using TaskFlow.Tasks.Domain.Events;

namespace TaskFlow.Notifications.Application.EventHandlers;

/// <summary>
/// CROSS-MODULE : écoute TaskCompletedEvent → félicite l'utilisateur.
/// 
/// IMPORTANT : un même événement peut avoir PLUSIEURS handlers.
/// TaskCompletedEvent pourrait aussi être écouté par :
/// - Un handler d'analytics (module futur)
/// - Un handler d'email
/// C'est la puissance du pattern Pub/Sub : 1 publisher, N subscribers.
/// </summary>
public sealed class OnTaskCompleted_CongratulateUser
    : INotificationHandler<TaskCompletedEvent>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationUnitOfWork _unitOfWork;

    public OnTaskCompleted_CongratulateUser(
        INotificationRepository repository,
        INotificationUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TaskCompletedEvent domainEvent, CancellationToken cancellationToken)
    {
        var notif = Notification.Create(
            domainEvent.UserId,
            "Tâche terminée ! ✅",
            $"Félicitations ! Votre tâche \"{domainEvent.Title}\" est terminée.",
            NotificationType.TaskCompleted);

        await _repository.AddAsync(notif, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
