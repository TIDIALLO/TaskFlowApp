using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Users.Application.Interfaces;
using TaskFlow.Users.Infrastructure.Data;
using TaskFlow.Users.Infrastructure.Repositories;
using TaskFlow.Users.Infrastructure.Services;

namespace TaskFlow.Users.Infrastructure;

/// <summary>
/// Enregistre les services Infrastructure (DB, Repositories, Services externes).
/// 
/// Remarque : on reçoit IConfiguration en paramètre car Infrastructure
/// a besoin de la connection string et des settings JWT.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core avec SQL Server
        services.AddDbContext<UsersDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repository pattern : l'Application dépend de IUserRepository (abstraction),
        // ici on dit "quand quelqu'un demande IUserRepository, donne-lui UserRepository"
        // Scoped = une instance par requête HTTP (partagée avec le DbContext)
        services.AddScoped<IUserRepository, UserRepository>();

        // UnitOfWork encapsule le SaveChanges de EF Core
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
