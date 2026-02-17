namespace TaskFlow.Shared.Kernel.Results;

/// <summary>
/// Catégorise le type d'erreur pour mapper automatiquement vers le bon code HTTP.
/// Chaque valeur correspond à un status code HTTP précis.
/// </summary>
public enum ErrorType
{
    Validation,   // 400 Bad Request  — données invalides envoyées par le client
    NotFound,     // 404 Not Found    — ressource introuvable
    Conflict,     // 409 Conflict     — doublon (ex: email déjà utilisé)
    Unauthorized, // 401 Unauthorized — pas authentifié ou credentials invalides
    Forbidden     // 403 Forbidden    — authentifié mais pas autorisé
}

/// <summary>
/// Représente une erreur métier (domain error).
/// "Code" identifie l'erreur de façon unique (ex: "User.NotFound").
/// "Message" est le message lisible par un humain.
/// "Type" détermine quel code HTTP sera retourné au client.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    /// <summary>
    /// Représente l'absence d'erreur. Utilisé quand Result.IsSuccess == true.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Validation);
}