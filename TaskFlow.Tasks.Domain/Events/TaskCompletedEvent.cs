using TaskFlow.Shared.Kernel.Primitives;

namespace TaskFlow.Tasks.Domain.Events;

/// <summary>
/// Événement émis quand une tâche est COMPLÉTÉE (Done).
/// Le module Notifications s'en servira pour créer une notification de félicitations.
/// </summary>
public sealed record TaskCompletedEvent(
    Guid TaskId,
    string Title,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
