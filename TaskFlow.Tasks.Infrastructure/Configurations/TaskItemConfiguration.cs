using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Tasks.Domain.Entities;
using TaskFlow.Tasks.Domain.Enums;

namespace TaskFlow.Tasks.Infrastructure.Configurations;

/// <summary>
/// Configuration EF Core Fluent API pour TaskItem.
/// 
/// POURQUOI Fluent API et pas Data Annotations ([Required], [MaxLength]...) ?
/// 1. Les Data Annotations polluent le Domain avec des attributs d'infrastructure
/// 2. La Fluent API offre PLUS de possibilités (index composites, conversions...)
/// 3. Séparation des préoccupations : le Domain ne sait rien d'EF Core
/// 
/// IEntityTypeConfiguration&lt;T&gt; est appliquée automatiquement par
/// ApplyConfigurationsFromAssembly() dans le DbContext.
/// </summary>
public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(t => t.Id);

        // ═══════════════════════════════════════════════════════════
        // VALUE OBJECTS — Mappés via OwnsOne (Owned Types)
        // OwnsOne = "cette propriété n'est pas une entité séparée,
        // c'est une partie de TaskItem stockée dans la même table"
        // ═══════════════════════════════════════════════════════════

        builder.OwnsOne(t => t.Title, title =>
        {
            title.Property(x => x.Value)
                .HasColumnName("Title")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.OwnsOne(t => t.Description, desc =>
        {
            desc.Property(x => x.Value)
                .HasColumnName("Description")
                .HasMaxLength(2000);
        });

        // ═══════════════════════════════════════════════════════════
        // ENUMS — Stockés en string pour la lisibilité en DB
        // HasConversion<string>() convertit l'enum ↔ string automatiquement
        // En DB on verra "High" au lieu de "2" — bien plus clair pour le debug
        // ═══════════════════════════════════════════════════════════

        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // ═══════════════════════════════════════════════════════════
        // SCALAIRES
        // ═══════════════════════════════════════════════════════════

        builder.Property(t => t.DueDate);
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.CompletedAt);

        // UserId est une FK vers la table Users, mais on ne crée PAS
        // de navigation property (pas de public User Owner { get; })
        // car les modules sont ISOLÉS. La relation est au niveau DB seulement.
        builder.Property(t => t.UserId).IsRequired();

        // ═══════════════════════════════════════════════════════════
        // INDEX — Optimise les requêtes les plus fréquentes
        // "Donne-moi toutes les tâches de l'utilisateur X" est LA requête la plus courante
        // Sans index, SQL Server ferait un full table scan à chaque fois.
        // ═══════════════════════════════════════════════════════════
        builder.HasIndex(t => t.UserId);

        // Ignore DomainEvents — c'est une propriété in-memory, pas stockée en DB
        builder.Ignore(t => t.DomainEvents);
    }
}
