using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Tasks.Application.Interfaces;
using TaskFlow.Tasks.Infrastructure.Data;
using TaskFlow.Tasks.Infrastructure.Repositories;

namespace TaskFlow.Tasks.Infrastructure;

/// <summary>
/// Enregistre les services Infrastructure du module Tasks.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddTasksInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Même connection string que Users — c'est la MÊME base de données
        // En modular monolith, les modules partagent la DB mais ont des DbContexts séparés
        // Chaque DbContext ne "voit" que ses propres tables
        services.AddDbContext<TasksDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ITaskItemRepository, TaskItemRepository>();

        // ATTENTION : c'est TaskFlow.Tasks.Application.Interfaces.IUnitOfWork
        // PAS TaskFlow.Users.Application.Interfaces.IUnitOfWork
        // Les deux interfaces ont le même nom mais des namespaces différents
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
