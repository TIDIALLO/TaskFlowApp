using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Users.Domain.ValueObjects;

public sealed record Password
{
    // Stocke TOUJOURS le hash, jamais le mot de passe en clair.
    public string HashedValue { get; }

    private Password(string hashedValue) => HashedValue = hashedValue;

    /// <summary>
    /// Valide les règles métier du mot de passe (longueur, complexité).
    /// Ne fait PAS le hachage ici — le Domain ne connaît pas BCrypt (c'est Infrastructure).
    /// Retourne un Password "temporaire" avec le texte brut, qui sera hashé par le handler.
    /// </summary>
    public static Result<Password> Create(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result<Password>.Failure(new Error(
                "Password.Empty", "Password cannot be empty.", ErrorType.Validation));

        if (password.Length < 8)
            return Result<Password>.Failure(new Error(
                "Password.TooShort", "Password must be at least 8 characters.", ErrorType.Validation));

        if (!password.Any(char.IsUpper))
            return Result<Password>.Failure(new Error(
                "Password.NoUppercase", "Password must contain at least one uppercase letter.", ErrorType.Validation));

        if (!password.Any(char.IsDigit))
            return Result<Password>.Failure(new Error(
                "Password.NoDigit", "Password must contain at least one digit.", ErrorType.Validation));

        // On passe le mot de passe brut ici — le RegisterUserCommandHandler
        // appellera IPasswordHasher pour le hasher AVANT de sauvegarder.
        return Result<Password>.Success(new Password(password));
    }

    /// <summary>
    /// Crée un Password depuis un hash déjà existant (lu de la DB).
    /// Utilisé par EF Core et lors de la reconstruction depuis la persistence.
    /// </summary>
    public static Password FromHash(string hash) => new(hash);
}