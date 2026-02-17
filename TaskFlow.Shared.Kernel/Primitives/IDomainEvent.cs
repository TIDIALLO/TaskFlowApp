using MediatR;

namespace TaskFlow.Shared.Kernel.Primitives;

/// <summary>
/// Interface marqueur pour les Domain Events.
/// 
/// QU'EST-CE QU'UN DOMAIN EVENT ?
/// Un événement qui signale que quelque chose d'IMPORTANT s'est passé dans le domaine.
/// Exemples : "Un utilisateur s'est inscrit", "Une tâche a été complétée".
/// 
/// POURQUOI dans Shared.Kernel ?
/// Parce que les Domain Events sont le PONT entre les modules.
/// - Le module Tasks publie TaskCompletedEvent
/// - Le module Notifications écoute et crée une notification
/// Les deux modules référencent Shared.Kernel, pas l'un l'autre.
/// 
/// POURQUOI hériter de INotification (MediatR) ?
/// Pour utiliser le mécanisme Pub/Sub de MediatR :
/// _mediator.Publish(event) → tous les INotificationHandler<TEvent> sont appelés.
/// 
/// MediatR.Contracts est un package LÉGER (pas le MediatR complet).
/// Il ne contient QUE les interfaces (INotification, IRequest).
/// Ainsi Shared.Kernel reste léger.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Quand l'événement s'est produit.
    /// </summary>
    DateTime OccurredAt { get; }
}
