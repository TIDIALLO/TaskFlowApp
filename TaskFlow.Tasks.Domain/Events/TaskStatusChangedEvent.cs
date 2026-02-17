using TaskFlow.Shared.Kernel.Primitives;

namespace TaskFlow.Tasks.Domain.Events;

/// <summary>
/// Événement émis quand le statut d'une tâche CHANGE (peu importe la transition).
/// Plus générique que TaskCompletedEvent — utile pour un historique d'activité.
/// </summary>
public sealed record TaskStatusChangedEvent(
    Guid TaskId,
    string Title,
    string OldStatus,
    string NewStatus,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
