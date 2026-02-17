namespace TaskFlow.Shared.Contracts.Notifications;

/// <summary>
/// DTO renvoyé par l'API pour une notification.
/// Utilisé par le frontend ET le backend (Shared.Contracts).
/// </summary>
public sealed record NotificationResponse(
    Guid Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt);
