using System.Net.Http.Json;
using TaskFlow.Shared.Contracts.Notifications;

namespace TaskFlow.Client.Services;

/// <summary>
/// Service client pour les notifications.
/// Même pattern que TaskService : encapsule les appels HTTP vers l'API.
/// 
/// ENDPOINTS APPELÉS :
/// GET    /api/notifications          → GetAllAsync()
/// GET    /api/notifications/unread   → GetUnreadCountAsync()
/// PATCH  /api/notifications/{id}/read → MarkAsReadAsync()
/// PATCH  /api/notifications/read-all  → MarkAllAsReadAsync()
/// </summary>
public class NotificationService
{
    private readonly HttpClient _httpClient;

    public NotificationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Récupère toutes les notifications de l'utilisateur connecté.
    /// Le JWT dans le header identifie l'utilisateur.
    /// </summary>
    public async Task<List<NotificationResponse>> GetAllAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<NotificationResponse>>("api/notifications");
            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// Récupère le nombre de notifications non lues (pour le badge).
    /// Appelée en polling toutes les 30 secondes par le NavMenu.
    /// </summary>
    public async Task<int> GetUnreadCountAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<UnreadCountResponse>("api/notifications/unread");
            return result?.Count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>Marque une notification comme lue</summary>
    public async Task<ServiceResult> MarkAsReadAsync(Guid id)
    {
        var response = await _httpClient.PatchAsync($"api/notifications/{id}/read", null);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>();
            return ServiceResult.Fail(error?.Detail ?? "Impossible de marquer comme lue.");
        }
        return ServiceResult.Ok();
    }

    /// <summary>Marque toutes les notifications comme lues</summary>
    public async Task<ServiceResult> MarkAllAsReadAsync()
    {
        var response = await _httpClient.PatchAsync("api/notifications/read-all", null);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>();
            return ServiceResult.Fail(error?.Detail ?? "Impossible de tout marquer comme lu.");
        }
        return ServiceResult.Ok();
    }
}
