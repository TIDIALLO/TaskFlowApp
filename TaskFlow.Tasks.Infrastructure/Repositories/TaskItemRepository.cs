using Microsoft.EntityFrameworkCore;
using TaskFlow.Tasks.Application.Interfaces;
using TaskFlow.Tasks.Domain.Entities;
using TaskFlow.Tasks.Infrastructure.Data;

namespace TaskFlow.Tasks.Infrastructure.Repositories;

/// <summary>
/// Implémentation concrète du repository avec EF Core.
/// 
/// PATTERN REPOSITORY :
/// - Encapsule l'accès aux données derrière une interface
/// - L'Application ne sait pas si on utilise EF Core, Dapper, ou un fichier JSON
/// - Facilite les tests : on mock ITaskItemRepository au lieu de mocker DbContext
/// 
/// IMPORTANT : Add/Update/Remove ne font pas de SaveChanges !
/// C'est le UnitOfWork qui appelle SaveChanges.
/// Cela permet de faire plusieurs opérations dans une seule transaction.
/// </summary>
public class TaskItemRepository : ITaskItemRepository
{
    private readonly TasksDbContext _context;

    public TaskItemRepository(TasksDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<TaskItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt) // Les plus récentes en premier
            .ToListAsync(cancellationToken);
    }

    public void Add(TaskItem task)
    {
        _context.Tasks.Add(task);
    }

    public void Update(TaskItem task)
    {
        _context.Tasks.Update(task);
    }

    public void Remove(TaskItem task)
    {
        _context.Tasks.Remove(task);
    }
}
