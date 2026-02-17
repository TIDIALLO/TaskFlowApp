using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Notifications.Application.EventHandlers;

namespace TaskFlow.Notifications.Application;

/// <summary>
/// Enregistre les services Application du module Notifications.
/// 
/// POINT CLÉ : on enregistre les handlers MediatR de CET assembly.
/// C'est ce qui permet à MediatR de TROUVER les event handlers cross-module :
/// - OnUserRegistered_CreateWelcomeNotification
/// - OnTaskCreated_NotifyUser
/// - OnTaskCompleted_CongratulateUser
/// 
/// Sans cet enregistrement, MediatR ne scannerait QUE Users.Application
/// et Tasks.Application, et ne trouverait JAMAIS nos handlers !
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsApplication(this IServiceCollection services)
    {
        // Scanne TaskFlow.Notifications.Application.dll pour trouver :
        // - INotificationHandler<> (event handlers cross-module)
        // - IRequestHandler<> (command/query handlers)
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(OnTaskCreated_NotifyUser).Assembly));

        return services;
    }
}
