using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Users.Domain.ValueObjects;

public sealed record FullName
{
    public string FirstName { get; }
    public string LastName { get; }
    public string Complete => $"{FirstName} {LastName}";

    private FullName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static Result<FullName> Create(string? firstName, string? lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result<FullName>.Failure(new Error(
                "FullName.FirstNameEmpty", "First name is required.", ErrorType.Validation));

        if (string.IsNullOrWhiteSpace(lastName))
            return Result<FullName>.Failure(new Error(
                "FullName.LastNameEmpty", "Last name is required.", ErrorType.Validation));

        return Result<FullName>.Success(new FullName(firstName.Trim(), lastName.Trim()));
    }
}