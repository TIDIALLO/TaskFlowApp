using MediatR;
using TaskFlow.Notifications.Application.Interfaces;
using TaskFlow.Notifications.Domain.Entities;
using TaskFlow.Notifications.Domain.Enums;
using TaskFlow.Tasks.Domain.Events;

namespace TaskFlow.Notifications.Application.EventHandlers;

/// <summary>
/// CROSS-MODULE : écoute TaskCreatedEvent (module Tasks) → crée une notification.
/// 
/// DIFFÉRENCE avec OnUserRegistered :
/// - UserRegisteredNotification est un INotification MediatR classique
/// - TaskCreatedEvent est un IDomainEvent (notre interface dans Shared.Kernel)
///   qui HÉRITE de INotification → MediatR le traite de la même façon
/// 
/// MAIS le dispatch est différent :
/// - UserRegistered : publié explicitement dans le RegisterUserCommandHandler
/// - TaskCreated : publié AUTOMATIQUEMENT par le UnitOfWork après SaveChanges
///   (pattern "Dispatch after SaveChanges" qu'on a implémenté)
/// </summary>
public sealed class OnTaskCreated_NotifyUser
    : INotificationHandler<TaskCreatedEvent>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationUnitOfWork _unitOfWork;

    public OnTaskCreated_NotifyUser(
        INotificationRepository repository,
        INotificationUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TaskCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var notif = Notification.Create(
            domainEvent.UserId,
            "Nouvelle tâche créée",
            $"Votre tâche \"{domainEvent.Title}\" (priorité {domainEvent.Priority}) a été créée.",
            NotificationType.TaskCreated);

        await _repository.AddAsync(notif, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
