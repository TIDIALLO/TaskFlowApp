using Microsoft.EntityFrameworkCore;
using TaskFlow.Notifications.Application.Interfaces;
using TaskFlow.Notifications.Domain.Entities;
using TaskFlow.Notifications.Infrastructure.Data;

namespace TaskFlow.Notifications.Infrastructure.Repositories;

/// <summary>
/// Implémentation concrète du INotificationRepository avec EF Core.
/// Même pattern que TaskItemRepository.
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly NotificationsDbContext _context;

    public NotificationRepository(NotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Notifications.FindAsync([id], ct);
    }

    public async Task<List<Notification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)  // Récentes d'abord
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public async Task AddAsync(Notification notification, CancellationToken ct = default)
    {
        await _context.Notifications.AddAsync(notification, ct);
    }
}
