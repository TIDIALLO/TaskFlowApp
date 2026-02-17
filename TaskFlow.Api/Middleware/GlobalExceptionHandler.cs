using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Api.Middleware;

/// <summary>
/// Intercepte TOUTES les exceptions non gérées de l'application.
/// 
/// POURQUOI ?
/// Sans ce handler, une exception (ex: DB inaccessible) retourne :
/// - En dev : une stacktrace complète (faille de sécurité si exposé)
/// - En prod : un 500 vide (pas utile pour le debug)
/// 
/// Avec ce handler, on retourne TOUJOURS un ProblemDetails JSON structuré :
/// {
///     "type": "https://tools.ietf.org/html/rfc7807",
///     "title": "Internal Server Error",
///     "status": 500,
///     "detail": "..."
/// }
/// 
/// ProblemDetails est un STANDARD (RFC 7807) utilisé par Microsoft.
/// C'est le format recommandé pour les erreurs d'API REST.
/// 
/// IExceptionHandler est l'interface .NET 8 pour gérer les exceptions globalement.
/// C'est plus propre que l'ancien middleware try/catch.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Logger l'exception complète (stacktrace) côté serveur pour le debug
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        // Créer un ProblemDetails standard
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            // En production, on ne montre PAS le message d'exception au client
            // (il pourrait contenir des infos sensibles comme des noms de tables SQL)
            Detail = "An unexpected error occurred. Please try again later."
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // Écrire le JSON dans la réponse HTTP
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // true = "j'ai géré l'exception, pas besoin de la propager plus loin"
        return true;
    }
}
