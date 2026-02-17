using MediatR;
using TaskFlow.Notifications.Application.Interfaces;
using TaskFlow.Notifications.Domain.Entities;
using TaskFlow.Notifications.Domain.Enums;
using TaskFlow.Users.Application.Notifications;

namespace TaskFlow.Notifications.Application.EventHandlers;

/// <summary>
/// CROSS-MODULE EVENT HANDLER — écoute un événement du module Users.
/// 
/// COMMENT ÇA MARCHE (c'est LE pattern clé du Modular Monolith) :
/// 
/// 1. Module Users : RegisterUserCommandHandler crée un User
///    → Publie UserRegisteredNotification via MediatR
/// 
/// 2. MediatR scanne TOUS les assemblies (configuré dans Program.cs)
///    → Trouve ce handler car il implémente INotificationHandler<UserRegisteredNotification>
/// 
/// 3. Ce handler (dans le module Notifications) est exécuté
///    → Crée une Notification de bienvenue dans SA propre base (DbContext séparé)
/// 
/// RÉSULTAT : Le module Users ne sait RIEN du module Notifications.
/// Il publie un événement, et c'est tout. S'il n'y a pas de handler, rien ne se passe.
/// C'est le principe de DÉCOUPLAGE (loose coupling).
/// 
/// NOMMAGE : On_EventSource_Action pour être explicite sur la provenance.
/// </summary>
public sealed class OnUserRegistered_CreateWelcomeNotification
    : INotificationHandler<UserRegisteredNotification>
{
    private readonly INotificationRepository _repository;
    private readonly INotificationUnitOfWork _unitOfWork;

    public OnUserRegistered_CreateWelcomeNotification(
        INotificationRepository repository,
        INotificationUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UserRegisteredNotification notification,
        CancellationToken cancellationToken)
    {
        var welcomeNotif = Notification.Create(
            notification.UserId,
            "Bienvenue sur TaskFlow ! 🎉",
            $"Bonjour {notification.FullName}, votre compte a été créé avec succès. " +
            "Commencez par créer votre première tâche !",
            NotificationType.Welcome);

        await _repository.AddAsync(welcomeNotif, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
