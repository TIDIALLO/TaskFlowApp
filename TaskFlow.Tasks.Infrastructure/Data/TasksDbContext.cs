using Microsoft.EntityFrameworkCore;
using TaskFlow.Tasks.Domain.Entities;

namespace TaskFlow.Tasks.Infrastructure.Data;

/// <summary>
/// DbContext du module Tasks.
/// 
/// POURQUOI un DbContext séparé par module ?
/// Dans une architecture modulaire (Modular Monolith), chaque module a son propre
/// DbContext pour :
/// 1. Isolation : les modules ne peuvent pas accéder aux tables des autres modules
/// 2. Évolutivité : si un jour on veut extraire un module en microservice, c'est facile
/// 3. Responsabilité : chaque DbContext ne gère que SES entités
/// 
/// ApplyConfigurationsFromAssembly scanne toutes les classes IEntityTypeConfiguration
/// de cet assembly et les applique automatiquement.
/// </summary>
public class TasksDbContext : DbContext
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TasksDbContext).Assembly);
    }
}
