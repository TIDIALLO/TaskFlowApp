using TaskFlow.Notifications.Domain.Entities;
using TaskFlow.Notifications.Domain.Enums;

namespace TaskFlow.Notifications.Tests.Fixtures;

/// <summary>
/// Données de test pour le module Notifications.
/// </summary>
public static class NotificationFixtures
{
    public static readonly Guid ValidUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid OtherUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public const string ValidTitle = "Bienvenue sur TaskFlow !";
    public const string ValidMessage = "Votre compte a été créé avec succès.";

    /// <summary>
    /// Crée une notification non lue de type Welcome.
    /// </summary>
    public static Notification CreateUnread()
    {
        return Notification.Create(ValidUserId, ValidTitle, ValidMessage, NotificationType.Welcome);
    }

    /// <summary>
    /// Crée une notification déjà lue.
    /// </summary>
    public static Notification CreateRead()
    {
        var notif = CreateUnread();
        notif.MarkAsRead();
        return notif;
    }

    /// <summary>
    /// Crée une liste de notifications mixtes (lues et non lues).
    /// </summary>
    public static List<Notification> CreateMixedList()
    {
        var n1 = Notification.Create(ValidUserId, "Tâche créée", "Msg 1", NotificationType.TaskCreated);
        var n2 = Notification.Create(ValidUserId, "Tâche terminée", "Msg 2", NotificationType.TaskCompleted);
        n2.MarkAsRead(); // celle-ci est lue
        var n3 = Notification.Create(ValidUserId, "Bienvenue", "Msg 3", NotificationType.Welcome);
        return [n1, n2, n3];
    }
}
