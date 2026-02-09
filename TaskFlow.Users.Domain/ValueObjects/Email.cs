using System.Text.RegularExpressions;
using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Users.Domain.ValueObjects;

public sealed record Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<Email>.Failure(new Error("Email.Empty", "Email cannot be empty."));

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return Result<Email>.Failure(new Error("Email.Invalid", "Email format is invalid."));

        return Result<Email>.Success(new Email(email.ToLowerInvariant()));
    }
}