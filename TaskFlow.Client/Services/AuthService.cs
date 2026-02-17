using System.Net.Http.Json;
using Blazored.LocalStorage;
using TaskFlow.Client.Auth;
using TaskFlow.Shared.Contracts.Auth;

namespace TaskFlow.Client.Services;

/// <summary>
/// Service qui gère    login/register/logout côté client.
/// 
/// ARCHITECTURE BLAZOR :
/// Les pages Blazor ne font PAS d'appels HTTP directement.
/// Elles utilisent des SERVICES qui encapsulent les appels API.
/// C'est le même principe que les services Angular/React.
/// 
/// Page → Service → HttpClient → API Backend
/// </summary>
public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly JwtAuthStateProvider _authStateProvider;

    public AuthService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        JwtAuthStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    /// <summary>
    /// Connecte l'utilisateur : appelle l'API → stocke le token → notifie Blazor.
    /// </summary>
    public async Task<ServiceResult> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/users/login", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>();
            return ServiceResult.Fail(error?.Detail ?? "Login failed.");
        }

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // Stocker le JWT dans le localStorage du navigateur (persiste même si on ferme l'onglet)
        await _localStorage.SetItemAsStringAsync(JwtAuthStateProvider.TokenKey, auth!.Token);

        // Ajouter le token dans les headers HTTP pour les prochains appels
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.Token);

        // Notifier Blazor que l'utilisateur est maintenant connecté
        _authStateProvider.NotifyUserAuthentication(auth.Token);

        return ServiceResult.Ok();
    }

    /// <summary>
    /// Inscrit un nouvel utilisateur.
    /// </summary>
    public async Task<ServiceResult> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/users/register", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>();
            return ServiceResult.Fail(error?.Detail ?? "Registration failed.");
        }

        return ServiceResult.Ok();
    }

    /// <summary>
    /// Déconnecte : supprime le token → notifie Blazor → les pages [Authorize] deviennent inaccessibles.
    /// </summary>
    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(JwtAuthStateProvider.TokenKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
        _authStateProvider.NotifyUserLogout();
    }
}

/// <summary>
/// Résultat simple pour les services client.
/// Pas besoin du pattern Result complet côté client.
/// </summary>
public record ServiceResult(bool IsSuccess, string? ErrorMessage = null)
{
    public static ServiceResult Ok() => new(true);
    public static ServiceResult Fail(string message) => new(false, message);
}

/// <summary>
/// DTO pour lire les erreurs ProblemDetails renvoyées par l'API.
/// </summary>
public record ProblemDetailsDto(string? Type, string? Title, string? Detail, int? Status);
