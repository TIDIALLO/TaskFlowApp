using TaskFlow.Notifications.Domain.Entities;
using TaskFlow.Shared.Contracts.Notifications;

namespace TaskFlow.Notifications.Application.Mappings;

/// <summary>
/// Extension methods pour mapper Notification (entity) → NotificationResponse (DTO).
/// Même pattern que TaskItemMappings dans le module Tasks.
/// </summary>
public static class NotificationMappings
{
    public static NotificationResponse ToResponse(this Notification notification) => new(
        notification.Id,
        notification.Title,
        notification.Message,
        notification.Type.ToString(),
        notification.IsRead,
        notification.CreatedAt,
        notification.ReadAt);

    public static List<NotificationResponse> ToResponseList(this List<Notification> notifications) =>
        notifications.Select(n => n.ToResponse()).ToList();
}
