using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Users.Application.Behaviors;
using TaskFlow.Users.Application.Commands.Register;

namespace TaskFlow.Users.Application;

/// <summary>
/// Méthode d'extension pour enregistrer les services de la couche Application.
/// 
/// POURQUOI une méthode d'extension ?
/// - Encapsulation : chaque couche sait quels services elle fournit
/// - Program.cs reste propre : juste builder.Services.AddUsersApplication()
/// - Modularité : si tu ajoutes un module "Tasks", il aura son propre AddTasksApplication()
/// 
/// C'est une convention très courante en .NET senior. Chaque projet expose
/// ses services via une méthode d'extension sur IServiceCollection.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddUsersApplication(this IServiceCollection services)
    {
        // Enregistre tous les handlers MediatR de cet assembly (Commands + Queries + Notifications)
        // typeof(RegisterUserCommand).Assembly pointe vers TaskFlow.Users.Application.dll
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));

        // Enregistre tous les validateurs FluentValidation de cet assembly
        // Scanne automatiquement toutes les classes qui héritent de AbstractValidator<T>
        services.AddValidatorsFromAssembly(typeof(RegisterUserCommand).Assembly);

        // Enregistre le Pipeline Behavior de validation
        // typeof(ValidationBehavior<,>) avec les <,> = type ouvert (générique non résolu)
        // MediatR résoudra automatiquement les bons types à l'exécution
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services; // Retourne services pour permettre le chaînage : services.AddX().AddY()
    }
}
