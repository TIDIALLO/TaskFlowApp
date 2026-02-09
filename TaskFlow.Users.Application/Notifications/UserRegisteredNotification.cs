using MediatR;

namespace TaskFlow.Users.Application.Notifications;

public sealed record UserRegisteredNotification(
    Guid UserId,
    string Email,
    string FullName) : INotification;