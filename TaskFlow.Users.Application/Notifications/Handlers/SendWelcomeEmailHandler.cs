using MediatR;
using Microsoft.Extensions.Logging;

namespace TaskFlow.Users.Application.Notifications.Handlers;

public sealed class SendWelcomeEmailHandler
    : INotificationHandler<UserRegisteredNotification>
{
    private readonly ILogger<SendWelcomeEmailHandler> _logger;

    public SendWelcomeEmailHandler(ILogger<SendWelcomeEmailHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        UserRegisteredNotification notification,
        CancellationToken cancellationToken)
    {
        // TODO: Injecter un IEmailService et envoyer vraiment
        _logger.LogInformation(
            "📧 Welcome email sent to {Email} for user {UserId}",
            notification.Email,
            notification.UserId);

        return Task.CompletedTask;
    }
}
