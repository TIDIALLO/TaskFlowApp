using TaskFlow.Shared.Kernel.Primitives;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Users.Domain.Errors;
using TaskFlow.Users.Domain.ValueObjects;

namespace TaskFlow.Users.Domain.Entities;

public sealed class User : Entity
{
    public Email Email { get; private set; }
    public Password Password { get; private set; }
    public FullName FullName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User(Guid id, Email email, Password password, FullName fullName) : base(id)
    {
        Email = email;
        Password = password;
        FullName = fullName;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    // EF Core a besoin d'un constructeur sans paramètre
#pragma warning disable CS8618
    private User() { }
#pragma warning restore CS8618

    public static Result<User> Create(Email email, Password password, FullName fullName)
    {
        var user = new User(Guid.NewGuid(), email, password, fullName);
        return Result<User>.Success(user);
    }

    public Result ChangeEmail(Email newEmail)
    {
        Email = newEmail;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(UserErrors.Inactive);

        IsActive = false;
        return Result.Success();
    }
}