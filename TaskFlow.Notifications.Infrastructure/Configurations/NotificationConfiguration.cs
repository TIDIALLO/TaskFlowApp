using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Notifications.Domain.Entities;

namespace TaskFlow.Notifications.Infrastructure.Configurations;

/// <summary>
/// Configuration EF Core Fluent API pour Notification.
/// Même pattern que TaskItemConfiguration dans le module Tasks.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        // Stocke l'enum comme string en DB (plus lisible que des int)
        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        // Index pour les requêtes fréquentes :
        // "Donne-moi les notifications non lues de l'utilisateur X, triées par date"
        builder.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt })
            .HasDatabaseName("IX_Notifications_UserId_IsRead_CreatedAt");

        // Ignore DomainEvents — c'est une propriété en mémoire, pas en DB
        builder.Ignore(n => n.DomainEvents);
    }
}
