using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Users.Domain.ValueObjects;

public sealed record Password
{
    public string HashedValue { get; }

    private Password(string hashedValue) => HashedValue = hashedValue;

    public static Result<Password> Create(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result<Password>.Failure(new Error("Password.Empty", "Password cannot be empty."));

        if (password.Length < 8)
            return Result<Password>.Failure(new Error("Password.TooShort", "Password must be at least 8 characters."));

        // Le hash sera fait dans Infrastructure, ici on stocke juste
        return Result<Password>.Success(new Password(password));
    }

    public static Password FromHash(string hash) => new(hash);
}