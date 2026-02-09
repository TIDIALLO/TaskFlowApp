using TaskFlow.Shared.Kernel.Results;

namespace TaskFlow.Users.Domain.Errors;

public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "User was not found.");
    public static readonly Error EmailAlreadyExists = new("User.EmailExists", "Email is already registered.");
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Invalid email or password.");
    public static readonly Error Inactive = new("User.Inactive", "User account is inactive.");
}