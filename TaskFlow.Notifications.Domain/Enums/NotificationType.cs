namespace TaskFlow.Notifications.Domain.Enums;

/// <summary>
/// Types de notifications possibles.
/// Chaque type correspond à un événement métier différent.
/// Le frontend pourra afficher une icône/couleur différente selon le type.
/// </summary>
public enum NotificationType
{
    /// <summary>Un utilisateur s'est inscrit → message de bienvenue</summary>
    Welcome = 0,

    /// <summary>Une nouvelle tâche a été créée</summary>
    TaskCreated = 1,

    /// <summary>Une tâche a été complétée</summary>
    TaskCompleted = 2,

    /// <summary>Le statut d'une tâche a changé</summary>
    TaskStatusChanged = 3,

    /// <summary>Notification système générale</summary>
    System = 4
}
