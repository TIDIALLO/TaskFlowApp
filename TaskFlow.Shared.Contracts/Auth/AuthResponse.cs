namespace TaskFlow.Shared.Contracts.Auth;

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string FullName,
    string Token);
