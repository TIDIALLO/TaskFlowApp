namespace TaskFlow.Users.Application.DTOs;

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string FullName,
    string Token);