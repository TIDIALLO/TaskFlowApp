using TaskFlow.Shared.Kernel.Primitives;

namespace TaskFlow.Tasks.Domain.Events;

/// <summary>
/// Événement émis quand une tâche est CRÉÉE.
/// 
/// POURQUOI c'est un record ?
/// - Immutable : une fois l'événement créé, il ne change jamais
/// - Comparaison par valeur : utile pour les tests
/// - Concis : pas besoin de constructeur, getters, etc.
/// 
/// POURQUOI on met les données BRUTES (Guid, string) et pas l'entité TaskItem ?
/// Parce que le module Notifications ne connaît PAS l'entité TaskItem.
/// Il ne connaît que Shared.Kernel. On doit donc envoyer des types simples.
/// C'est le principe de DÉCOUPLAGE entre modules.
/// </summary>
public sealed record TaskCreatedEvent(
    Guid TaskId,
    string Title,
    string Priority,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
