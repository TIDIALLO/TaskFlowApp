using System.Text.RegularExpressions;
using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Users.Domain.ValueObjects;

public sealed partial record Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    // [GeneratedRegex] génère le code de la regex à la compilation.
    // C'est plus performant que Regex.IsMatch() qui recompile le pattern à chaque appel.
    // "partial" sur la classe est requis car le code source est généré automatiquement.
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<Email>.Failure(new Error(
                "Email.Empty", "Email cannot be empty.", ErrorType.Validation));

        if (!EmailRegex().IsMatch(email))
            return Result<Email>.Failure(new Error(
                "Email.Invalid", "Email format is invalid.", ErrorType.Validation));

        return Result<Email>.Success(new Email(email.ToLowerInvariant()));
    }
}