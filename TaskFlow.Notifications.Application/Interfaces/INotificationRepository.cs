using TaskFlow.Notifications.Domain.Entities;

namespace TaskFlow.Notifications.Application.Interfaces;

/// <summary>
/// Repository pattern pour les notifications.
/// Même principe que ITaskItemRepository :
/// - L'Application définit le contrat (QUOI)
/// - L'Infrastructure l'implémente (COMMENT — EF Core, SQL, etc.)
/// </summary>
public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Récupère les notifications d'un utilisateur, triées par date (récentes d'abord)</summary>
    Task<List<Notification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Compte les non-lues pour le badge</summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    Task AddAsync(Notification notification, CancellationToken ct = default);
}
