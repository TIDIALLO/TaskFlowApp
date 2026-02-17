using FluentAssertions;
using TaskFlow.Notifications.Domain.Entities;
using TaskFlow.Notifications.Domain.Enums;
using TaskFlow.Notifications.Tests.Fixtures;

namespace TaskFlow.Notifications.Tests.Domain.Entities;

/// <summary>
/// Tests unitaires pour l'entité Notification.
/// 
/// ON TESTE ICI :
/// 1. La factory method Create — création correcte des propriétés
/// 2. MarkAsRead — transition IsRead false → true avec date
/// 3. MarkAsRead idempotent — déjà lue → pas de changement
/// 
/// DIFFÉRENCE avec TaskItem :
/// Notification.Create ne retourne PAS un Result — les données
/// viennent d'event handlers (déjà validées). Pas de validation ici.
/// </summary>
public class NotificationTests
{
    // ═══════════════════════════════════════════════════════
    // FACTORY METHOD — Create
    // ═══════════════════════════════════════════════════════

    [Fact]
    public void Create_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange & Act
        var notification = Notification.Create(
            NotificationFixtures.ValidUserId,
            NotificationFixtures.ValidTitle,
            NotificationFixtures.ValidMessage,
            NotificationType.Welcome);

        // Assert
        notification.Id.Should().NotBeEmpty();
        notification.UserId.Should().Be(NotificationFixtures.ValidUserId);
        notification.Title.Should().Be(NotificationFixtures.ValidTitle);
        notification.Message.Should().Be(NotificationFixtures.ValidMessage);
        notification.Type.Should().Be(NotificationType.Welcome);
        notification.IsRead.Should().BeFalse();       // par défaut non lue
        notification.ReadAt.Should().BeNull();          // pas encore lue
        notification.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData(NotificationType.Welcome)]
    [InlineData(NotificationType.TaskCreated)]
    [InlineData(NotificationType.TaskCompleted)]
    [InlineData(NotificationType.TaskStatusChanged)]
    [InlineData(NotificationType.System)]
    public void Create_WithDifferentTypes_ShouldSetTypeCorrectly(NotificationType type)
    {
        // Act
        var notification = Notification.Create(
            NotificationFixtures.ValidUserId,
            "Titre",
            "Message",
            type);

        // Assert
        notification.Type.Should().Be(type);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Act — créer deux notifications
        var notif1 = NotificationFixtures.CreateUnread();
        var notif2 = NotificationFixtures.CreateUnread();

        // Assert — chaque notification a un Id unique
        notif1.Id.Should().NotBe(notif2.Id);
    }

    // ═══════════════════════════════════════════════════════
    // MarkAsRead — transition
    // ═══════════════════════════════════════════════════════

    [Fact]
    public void MarkAsRead_WhenUnread_ShouldSetIsReadTrueAndFillReadAt()
    {
        // Arrange
        var notification = NotificationFixtures.CreateUnread();
        notification.IsRead.Should().BeFalse();

        // Act
        notification.MarkAsRead();

        // Assert
        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().NotBeNull();
        notification.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void MarkAsRead_WhenAlreadyRead_ShouldBeIdempotent()
    {
        // Arrange — notification déjà lue
        var notification = NotificationFixtures.CreateRead();
        var originalReadAt = notification.ReadAt;

        // Act — marquer comme lue une 2ème fois
        notification.MarkAsRead();

        // Assert — RIEN ne change (idempotent)
        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().Be(originalReadAt); // la date ne change pas
    }

    // ═══════════════════════════════════════════════════════
    // ENTITY — Identity (hérité de Entity base class)
    // ═══════════════════════════════════════════════════════

    [Fact]
    public void TwoNotifications_WithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var notif1 = NotificationFixtures.CreateUnread();
        var notif2 = NotificationFixtures.CreateUnread();

        // Assert — Entity compare par Id
        notif1.Should().NotBe(notif2);
    }
}
