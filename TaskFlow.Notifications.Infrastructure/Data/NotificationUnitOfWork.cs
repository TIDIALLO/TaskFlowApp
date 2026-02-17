using TaskFlow.Notifications.Application.Interfaces;

namespace TaskFlow.Notifications.Infrastructure.Data;

/// <summary>
/// UnitOfWork simple pour Notifications.
/// Pas de dispatch de Domain Events ici car les notifications
/// ne lèvent pas d'events (elles SONT le résultat d'events).
/// </summary>
public class NotificationUnitOfWork : INotificationUnitOfWork
{
    private readonly NotificationsDbContext _context;

    public NotificationUnitOfWork(NotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
