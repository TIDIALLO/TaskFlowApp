namespace TaskFlow.Notifications.Application.Interfaces;

/// <summary>
/// Unit of Work pour le module Notifications.
/// Chaque module a son PROPRE UnitOfWork → chaque module a sa propre transaction.
/// </summary>
public interface INotificationUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
