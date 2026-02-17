using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Tasks.Application.Commands.CreateTask;

namespace TaskFlow.Tasks.Application;

/// <summary>
/// Enregistre les services Application du module Tasks.
/// Même pattern que pour Users : chaque module expose son propre AddXxxApplication().
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddTasksApplication(this IServiceCollection services)
    {
        // typeof(CreateTaskCommand).Assembly pointe vers TaskFlow.Tasks.Application.dll
        var assembly = typeof(CreateTaskCommand).Assembly;

        // Enregistre les handlers MediatR de CE module
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Enregistre les validateurs FluentValidation de CE module
        services.AddValidatorsFromAssembly(assembly);

        // Note : le ValidationBehavior est déjà enregistré dans AddUsersApplication().
        // MediatR le partage automatiquement car il est enregistré comme type ouvert
        // typeof(IPipelineBehavior<,>). Pas besoin de le ré-enregistrer ici.

        return services;
    }
}
