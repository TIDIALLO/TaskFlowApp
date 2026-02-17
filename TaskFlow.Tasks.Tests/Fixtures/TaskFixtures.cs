using TaskFlow.Tasks.Domain.Entities;
using TaskFlow.Tasks.Domain.Enums;
using TaskFlow.Tasks.Domain.ValueObjects;

namespace TaskFlow.Tasks.Tests.Fixtures;

/// <summary>
/// Données de test réutilisables pour le module Tasks.
/// 
/// POURQUOI un Fixtures ?
/// - DRY : on ne répète pas les mêmes données dans chaque test
/// - Lisibilité : TaskFixtures.ValidTitle est plus clair que "Ma tâche"
/// - Maintenabilité : si les règles changent, on modifie un seul endroit
/// </summary>
public static class TaskFixtures
{
    // ── Données valides ──
    public const string ValidTitle = "Implémenter le module Notifications";
    public const string ValidDescription = "Créer le domain, application et infrastructure layers";
    public const string ValidPriority = "High";
    public static readonly Guid ValidUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly DateTime ValidDueDate = DateTime.UtcNow.AddDays(7);

    // ── Données invalides ──
    public const string EmptyTitle = "";
    public static readonly string TooLongTitle = new('A', TaskTitle.MaxLength + 1);
    public static readonly string TooLongDescription = new('B', TaskDescription.MaxLength + 1);
    public static readonly DateTime PastDueDate = DateTime.UtcNow.AddDays(-1);

    // ── Helpers pour créer des entités valides ──

    /// <summary>
    /// Crée un TaskItem valide en statut Todo, prêt à être utilisé dans les tests.
    /// </summary>
    public static TaskItem CreateValidTask()
    {
        var title = TaskTitle.Create(ValidTitle).Value;
        var desc = TaskDescription.Create(ValidDescription).Value;
        var result = TaskItem.Create(title, desc, Priority.High, ValidDueDate, ValidUserId);
        return result.Value;
    }

    /// <summary>
    /// Crée un TaskItem valide puis le démarre (InProgress).
    /// </summary>
    public static TaskItem CreateStartedTask()
    {
        var task = CreateValidTask();
        task.Start();
        return task;
    }

    /// <summary>
    /// Crée un TaskItem valide puis le complète (Done).
    /// </summary>
    public static TaskItem CreateCompletedTask()
    {
        var task = CreateStartedTask();
        task.Complete();
        return task;
    }

    /// <summary>
    /// Crée un TaskItem valide puis l'annule (Cancelled).
    /// </summary>
    public static TaskItem CreateCancelledTask()
    {
        var task = CreateValidTask();
        task.Cancel();
        return task;
    }
}
