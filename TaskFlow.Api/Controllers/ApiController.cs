using Microsoft.AspNetCore.Mvc;
using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Api.Controllers;

/// <summary>
/// Contrôleur de base dont héritent tous les contrôleurs de l'API.
/// 
/// POURQUOI un contrôleur de base ?
/// Pour centraliser la conversion Result → IActionResult.
/// Chaque contrôleur n'a qu'à appeler HandleResult() sans se soucier
/// du mapping erreur → code HTTP. C'est le principe DRY (Don't Repeat Yourself).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    /// <summary>
    /// Convertit un Result&lt;T&gt; (succès avec valeur) en réponse HTTP 200 OK.
    /// </summary>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return HandleError(result.Error);
    }

    /// <summary>
    /// Convertit un Result&lt;T&gt; en réponse HTTP 201 Created.
    /// Utilisé pour les endpoints de création (POST).
    /// 
    /// actionName = nom de l'action GET pour retrouver la ressource créée
    /// routeValues = paramètres de la route (ex: { id = user.Id })
    /// Ensemble, ils génèrent le header "Location: /api/users/{id}"
    /// </summary>
    protected IActionResult HandleCreatedResult<T>(
        Result<T> result,
        string actionName,
        object routeValues)
    {
        if (result.IsSuccess)
            return CreatedAtAction(actionName, routeValues, result.Value);

        return HandleError(result.Error);
    }

    /// <summary>
    /// Convertit un Result simple (sans valeur) en réponse HTTP 204 No Content.
    /// Utilisé pour les opérations qui ne retournent rien (ex: Delete, Update).
    /// </summary>
    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return HandleError(result.Error);
    }

    /// <summary>
    /// Mappe un ErrorType vers le bon code HTTP.
    /// 
    /// AVANT : on faisait du pattern matching sur des strings ("Contains("Empty")")
    /// → fragile, un oubli et le mauvais code HTTP est retourné.
    /// 
    /// MAINTENANT : l'ErrorType enum dit explicitement quel type d'erreur c'est.
    /// Le mapping est garanti correct et exhaustif grâce au "switch expression"
    /// avec le pattern _ (default) qui attrape les cas oubliés.
    /// </summary>
    private IActionResult HandleError(Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation   => BadRequest(CreateProblemDetails(error, StatusCodes.Status400BadRequest)),
            ErrorType.NotFound     => NotFound(CreateProblemDetails(error, StatusCodes.Status404NotFound)),
            ErrorType.Conflict     => Conflict(CreateProblemDetails(error, StatusCodes.Status409Conflict)),
            ErrorType.Unauthorized => Unauthorized(CreateProblemDetails(error, StatusCodes.Status401Unauthorized)),
            ErrorType.Forbidden    => StatusCode(StatusCodes.Status403Forbidden,
                                        CreateProblemDetails(error, StatusCodes.Status403Forbidden)),
            _ => BadRequest(CreateProblemDetails(error, StatusCodes.Status400BadRequest))
        };
    }

    /// <summary>
    /// Crée un ProblemDetails standard (RFC 7807) pour toutes les réponses d'erreur.
    /// Uniformise le format de toutes les erreurs de l'API.
    /// </summary>
    private static ProblemDetails CreateProblemDetails(Error error, int statusCode) => new()
    {
        Type = $"https://httpstatuses.com/{statusCode}",
        Title = error.Code,
        Detail = error.Message,
        Status = statusCode
    };
}
