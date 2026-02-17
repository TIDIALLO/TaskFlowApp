namespace TaskFlow.Shared.Contracts.Notifications;

/// <summary>
/// DTO pour le compteur de notifications non lues.
/// Le NavMenu l'appelle en polling pour afficher le badge.
/// </summary>
public sealed record UnreadCountResponse(int Count);
