using TaskFlow.Tasks.Domain.Entities;

namespace TaskFlow.Tasks.Application.Interfaces;

/// <summary>
/// Contrat du repository pour les tâches.
/// 
/// POURQUOI cette interface est dans Application et pas dans Domain ?
/// En Clean Architecture stricte, les interfaces de repository PEUVENT être dans le Domain.
/// Mais ici on les met dans Application car :
/// 1. C'est la couche Application qui en a besoin (les handlers)
/// 2. Le Domain reste pur — que des entités, value objects, et règles métier
/// 3. C'est la convention la plus courante dans les projets .NET modernes
/// </summary>
public interface ITaskItemRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<TaskItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    void Add(TaskItem task);
    void Update(TaskItem task);
    void Remove(TaskItem task);
}
