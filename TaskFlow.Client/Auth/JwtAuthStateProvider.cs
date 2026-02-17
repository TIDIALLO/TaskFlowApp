using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace TaskFlow.Client.Auth;

/// <summary>
/// AuthenticationStateProvider personnalisé pour JWT.
/// 
/// COMMENT ÇA MARCHE ?
/// En Blazor, le framework a besoin de savoir "qui est connecté" pour :
/// 1. Afficher/cacher des éléments ([Authorize] dans les pages)
/// 2. Rediriger vers Login si non authentifié
/// 3. Fournir les claims de l'utilisateur (Id, Email, Name...)
/// 
/// Ce provider :
/// - Lit le JWT depuis le localStorage du navigateur
/// - Décode les claims du JWT (sans appel serveur — le JWT est auto-contenu)
/// - Retourne un AuthenticationState qui dit "connecté" ou "anonyme"
/// 
/// QUAND est-il appelé ?
/// - Au démarrage de l'app (pour restaurer la session)
/// - Quand on appelle NotifyUserAuthentication/NotifyUserLogout
/// - À chaque navigation vers une page [Authorize]
/// </summary>
public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;

    // Un state "anonyme" par défaut (pas connecté)
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public const string TokenKey = "authToken";

    public JwtAuthStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Appelé par Blazor pour connaître l'état d'authentification actuel.
    /// Lit le token JWT du localStorage et en extrait les claims.
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync(TokenKey);

        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        // Nettoyer les guillemets éventuels (le JSON serializer les ajoute parfois)
        token = token.Trim('"');

        // Vérifier si le token est expiré
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        if (jwt.ValidTo < DateTime.UtcNow)
        {
            await _localStorage.RemoveItemAsync(TokenKey);
            return Anonymous;
        }

        // Ajouter le token dans le header Authorization pour toutes les requêtes HTTP
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Créer un ClaimsPrincipal avec les claims du JWT
        // ClaimsIdentity("jwt") = le nom du schéma d'auth → dit à Blazor "cet utilisateur est authentifié"
        // Si on passe un string vide → Blazor considère l'utilisateur comme anonyme
        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    /// <summary>
    /// Appelé après un login réussi pour mettre à jour l'état d'auth.
    /// NotifyAuthenticationStateChanged déclenche un re-render de tous les
    /// composants qui dépendent de l'état d'auth (AuthorizeView, etc).
    /// </summary>
    public void NotifyUserAuthentication(string token)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        var state = Task.FromResult(new AuthenticationState(user));
        NotifyAuthenticationStateChanged(state);
    }

    /// <summary>
    /// Appelé après un logout pour remettre l'état à "anonyme".
    /// </summary>
    public void NotifyUserLogout()
    {
        var state = Task.FromResult(Anonymous);
        NotifyAuthenticationStateChanged(state);
    }
}
