using TaskFlow.Notifications.Domain.Enums;
using TaskFlow.Shared.Kernel.Primitives;

namespace TaskFlow.Notifications.Domain.Entities;

/// <summary>
/// Entité Notification — le cœur du module Notifications.
/// 
/// MÊME PATTERNS que TaskItem :
/// - Constructeur privé + Factory method (Create)
/// - private set → encapsulation stricte
/// - Méthodes métier (MarkAsRead)
/// - Hérite de Entity (Id + DomainEvents)
/// 
/// DIFFÉRENCE CLÉE avec TaskItem :
/// Cette entité est créée AUTOMATIQUEMENT par des event handlers,
/// pas par un utilisateur via un endpoint. C'est le module Tasks/Users
/// qui déclenche la création via des Domain Events.
/// </summary>
public sealed class Notification : Entity
{
    /// <summary>L'utilisateur destinataire de la notification</summary>
    public Guid UserId { get; private set; }

    /// <summary>Titre court affiché dans la cloche (ex: "Tâche complétée")</summary>
    public string Title { get; private set; }

    /// <summary>Message détaillé (ex: "Votre tâche 'Fix bug' est terminée")</summary>
    public string Message { get; private set; }

    /// <summary>Le type (Welcome, TaskCreated, TaskCompleted...)</summary>
    public NotificationType Type { get; private set; }

    /// <summary>Lue ou non — par défaut false</summary>
    public bool IsRead { get; private set; }

    /// <summary>Date de création</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Date de lecture (null si pas encore lue)</summary>
    public DateTime? ReadAt { get; private set; }

    private Notification(
        Guid id,
        Guid userId,
        string title,
        string message,
        NotificationType type) : base(id)
    {
        UserId = userId;
        Title = title;
        Message = message;
        Type = type;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

#pragma warning disable CS8618
    private Notification() { }
#pragma warning restore CS8618

    /// <summary>
    /// Factory method — crée une notification.
    /// Pas de Result ici car les données viennent d'un event handler (déjà validées).
    /// </summary>
    public static Notification Create(
        Guid userId,
        string title,
        string message,
        NotificationType type)
    {
        return new Notification(Guid.NewGuid(), userId, title, message, type);
    }

    /// <summary>
    /// Marque la notification comme lue.
    /// INVARIANT : si déjà lue, on ne fait rien (idempotent).
    /// </summary>
    public void MarkAsRead()
    {
        if (IsRead) return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
