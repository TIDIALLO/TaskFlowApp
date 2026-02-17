namespace TaskFlow.Shared.Kernel.Primitives;

/// <summary>
/// Classe de base pour toutes les entités du domaine.
/// 
/// NOUVEAU : supporte les Domain Events.
/// Quand une entité fait quelque chose d'important (ex: task.Complete()),
/// elle enregistre un événement dans _domainEvents.
/// Après SaveChanges, le UnitOfWork publie ces événements via MediatR.
/// 
/// C'est le pattern "Raise Domain Events" :
/// 1. L'entité enregistre l'événement (Raise)
/// 2. L'Infrastructure le publie après la persistance (Dispatch)
/// 3. Les handlers dans d'autres modules réagissent (Handle)
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; protected set; }

    // ═══════════════════════════════════════════════════════════
    // DOMAIN EVENTS — Liste des événements en attente de publication
    // ═══════════════════════════════════════════════════════════
    
    /// <summary>
    /// Événements accumulés par l'entité, pas encore publiés.
    /// Private set : seule l'entité peut ajouter des events (via Raise).
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Lecture seule depuis l'extérieur. Le UnitOfWork lira cette liste
    /// après SaveChanges pour publier les events via MediatR.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// L'entité enregistre un événement. Appelé depuis les méthodes métier.
    /// Ex: dans Complete() → Raise(new TaskCompletedEvent(...))
    /// </summary>
    protected void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Vide la liste après publication. Appelé par le UnitOfWork.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    // ═══════════════════════════════════════════════════════════
    // IDENTITY — Comparaison par Id
    // ═══════════════════════════════════════════════════════════

    protected Entity(Guid id)
    {
        Id = id;
    }

    protected Entity() { }

    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Entity);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}
