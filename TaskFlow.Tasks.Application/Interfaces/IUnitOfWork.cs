namespace TaskFlow.Tasks.Application.Interfaces;

/// <summary>
/// UnitOfWork pour le module Tasks.
/// Chaque module a son propre IUnitOfWork car chaque module a son propre DbContext.
/// C'est une conséquence de l'architecture modulaire : les modules sont indépendants.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
