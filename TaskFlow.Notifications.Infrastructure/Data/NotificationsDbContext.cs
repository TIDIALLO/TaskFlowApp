using Microsoft.EntityFrameworkCore;
using TaskFlow.Notifications.Domain.Entities;

namespace TaskFlow.Notifications.Infrastructure.Data;

/// <summary>
/// DbContext ISOLÉ pour le module Notifications.
/// 
/// PATTERN MODULAR MONOLITH : chaque module a SON propre DbContext.
/// Tous utilisent la MÊME base de données (même connection string),
/// MAIS chaque DbContext ne voit QUE ses propres tables.
/// 
/// - TasksDbContext → table TaskItems
/// - UsersDbContext → table Users
/// - NotificationsDbContext → table Notifications  ← NOUVEAU
/// 
/// POURQUOI ne pas utiliser UN seul DbContext pour tout ?
/// → Couplage fort : si le module Tasks change son schéma, ça peut casser Notifications
/// → Difficulté à extraire un module en microservice plus tard
/// → Violation du principe de responsabilité unique (SRP)
/// </summary>
public class NotificationsDbContext : DbContext
{
    public DbSet<Notification> Notifications => Set<Notification>();

    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Applique toutes les configurations IEntityTypeConfiguration de cet assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
    }
}
