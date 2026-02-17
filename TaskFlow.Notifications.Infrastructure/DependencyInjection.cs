using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Notifications.Application.Interfaces;
using TaskFlow.Notifications.Infrastructure.Data;
using TaskFlow.Notifications.Infrastructure.Repositories;

namespace TaskFlow.Notifications.Infrastructure;

/// <summary>
/// Extension method pour enregistrer les services du module Notifications.
/// Appelé dans Program.cs : builder.Services.AddNotificationsModule(config);
/// 
/// PATTERN : chaque module expose UNE méthode d'extension Add{Module}Module().
/// Program.cs n'a pas besoin de connaître les détails internes du module.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext isolé pour Notifications — même connection string, tables différentes
        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repository : INotificationRepository → NotificationRepository
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // UnitOfWork : INotificationUnitOfWork → NotificationUnitOfWork
        services.AddScoped<INotificationUnitOfWork, NotificationUnitOfWork>();

        return services;
    }
}
