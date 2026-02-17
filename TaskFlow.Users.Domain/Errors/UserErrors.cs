using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Users.Domain.Errors;

/// <summary>
/// Centralise toutes les erreurs métier liées au module Users.
/// Chaque erreur a un Code unique, un Message, et un ErrorType
/// qui détermine le code HTTP retourné (404, 409, 401...).
/// </summary>
public static class UserErrors
{
    public static readonly Error NotFound = new(
        "User.NotFound", "User was not found.", ErrorType.NotFound);

    public static readonly Error EmailAlreadyExists = new(
        "User.EmailExists", "Email is already registered.", ErrorType.Conflict);

    public static readonly Error InvalidCredentials = new(
        "User.InvalidCredentials", "Invalid email or password.", ErrorType.Unauthorized);

    public static readonly Error Inactive = new(
        "User.Inactive", "User account is inactive.", ErrorType.Forbidden);
}