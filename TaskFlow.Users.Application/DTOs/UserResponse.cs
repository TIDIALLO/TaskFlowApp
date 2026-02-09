namespace TaskFlow.Users.Application.DTOs;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    DateTime CreatedAt);