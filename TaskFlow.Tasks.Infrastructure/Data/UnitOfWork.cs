using MediatR;
using TaskFlow.Shared.Kernel.Primitives;
using TaskFlow.Tasks.Application.Interfaces;

namespace TaskFlow.Tasks.Infrastructure.Data;

/// <summary>
/// UnitOfWork avec dispatch des Domain Events.
/// 
/// PATTERN "Dispatch after SaveChanges" :
/// 1. Le Handler fait ses modifications (Add, Update...)
/// 2. Le Handler appelle _unitOfWork.SaveChangesAsync()
/// 3. SaveChanges écrit en base de données
/// 4. APRÈS le commit, on publie les Domain Events via MediatR
/// 
/// POURQUOI "après" et pas "avant" SaveChanges ?
/// Si on publie AVANT et que SaveChanges échoue (ex: DB down),
/// le module Notifications aurait créé une notification pour une tâche
/// qui n'existe PAS en base. En publiant APRÈS, on est sûr que les
/// données sont persistées avant de notifier.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly TasksDbContext _context;
    private readonly IMediator _mediator;

    public UnitOfWork(TasksDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Sauvegarder en base
        var result = await _context.SaveChangesAsync(cancellationToken);

        // 2. Récupérer toutes les entités qui ont des Domain Events en attente
        //    ChangeTracker.Entries() donne accès à toutes les entités suivies par EF Core
        var entitiesWithEvents = _context.ChangeTracker
            .Entries<Entity>()                          // Toutes les entités (Entity est la base)
            .Where(e => e.Entity.DomainEvents.Any())    // Qui ont au moins un event
            .Select(e => e.Entity)
            .ToList();

        // 3. Collecter tous les events AVANT de les clear (sinon on perd la liste)
        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // 4. Vider les events des entités (ils vont être publiés)
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        // 5. Publier chaque event via MediatR
        //    MediatR trouvera TOUS les INotificationHandler<TEvent> enregistrés
        //    (même dans d'autres modules !)
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}
