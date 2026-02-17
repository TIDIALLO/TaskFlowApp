using FluentValidation;
using MediatR;
using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Users.Application.Behaviors;

/// <summary>
/// Pipeline Behavior MediatR pour la validation automatique.
/// 
/// COMMENT ÇA MARCHE ?
/// MediatR envoie chaque Request à travers une chaîne de "behaviors" (comme des middlewares)
/// AVANT d'atteindre le handler. Ce behavior :
/// 1. Récupère tous les validateurs enregistrés pour cette Request
/// 2. Les exécute tous
/// 3. Si des erreurs → retourne un Result.Failure (le handler n'est JAMAIS appelé)
/// 4. Si pas d'erreurs → appelle next() qui passe au behavior suivant ou au handler
/// 
/// Le pipeline ressemble à ça :
/// Request → [ValidationBehavior] → [LoggingBehavior] → Handler → Response
///                   ↓ (si erreur)
///              Result.Failure
/// 
/// TRequest = le type de la commande/query (ex: RegisterUserCommand)
/// TResponse = le type de retour (ex: Result&lt;UserResponse&gt;)
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result // On contraint TResponse à être un Result (ou Result<T>)
{
    // IEnumerable<IValidator<TRequest>> : MediatR injecte TOUS les validateurs
    // enregistrés pour ce type de Request. S'il n'y en a pas, la liste est vide.
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next, // next() = appelle le prochain behavior ou le handler
        CancellationToken cancellationToken)
    {
        // S'il n'y a aucun validateur pour cette Request, on passe directement au handler
        if (!_validators.Any())
            return await next();

        // Exécuter tous les validateurs en parallèle
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collecter toutes les erreurs de tous les validateurs
        var errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        // S'il y a des erreurs, on retourne un Failure SANS appeler le handler
        if (errors.Count != 0)
        {
            // On prend la première erreur pour le Result
            // (dans une version avancée, on pourrait retourner toutes les erreurs)
            var firstError = errors.First();
            var error = new Error(firstError.PropertyName, firstError.ErrorMessage, ErrorType.Validation);

            // On doit créer un Result du bon type (Result<T>).
            // CreateFailure est une méthode helper statique sur Result.
            return CreateFailureResult(error);
        }

        // Pas d'erreurs → on continue vers le handler
        return await next();
    }

    /// <summary>
    /// Crée un Result.Failure du type approprié (Result ou Result&lt;T&gt;) par réflexion.
    /// 
    /// Pourquoi la réflexion ? Parce que TResponse peut être Result ou Result&lt;UserResponse&gt;
    /// et on ne sait pas à la compilation quel type exact c'est.
    /// </summary>
    private static TResponse CreateFailureResult(Error error)
    {
        // Si TResponse est Result<T>, on utilise Result<T>.Failure(error)
        // Si TResponse est Result, on utilise Result.Failure(error)
        var resultType = typeof(TResponse);

        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            // Appeler Result<T>.Failure(error) via réflexion
            var failureMethod = resultType.GetMethod(nameof(Result.Failure), new[] { typeof(Error) });
            return (TResponse)failureMethod!.Invoke(null, new object[] { error })!;
        }

        // C'est un Result simple (non générique)
        return (TResponse)(object)Result.Failure(error);
    }
}
