using MediatR;

namespace TaskFlow.Users.Application.Notifications.Handlers;

public sealed class LogUserRegistrationHandler
    : INotificationHandler<UserRegisteredNotification>
{
    public Task Handle(
        UserRegisteredNotification notification,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"📝 User registered: {notification.UserId} - {notification.FullName}");
        return Task.CompletedTask;
    }
}