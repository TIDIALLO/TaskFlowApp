using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TaskFlow.Client;
using TaskFlow.Client.Auth;
using TaskFlow.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ═══════════════════════════════════════════════════════════
// HTTP CLIENT — Pointe vers l'API backend
// ═══════════════════════════════════════════════════════════
// L'URL de base est celle de l'API. Toutes les requêtes HttpClient
// utiliseront cette URL comme préfixe.
// Ex: httpClient.GetAsync("api/tasks") → GET https://localhost:7200/api/tasks
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7239")
});

// ═══════════════════════════════════════════════════════════
// BLAZORED LOCAL STORAGE — Stockage JWT dans le navigateur
// ═══════════════════════════════════════════════════════════
builder.Services.AddBlazoredLocalStorage();

// ═══════════════════════════════════════════════════════════
// AUTHENTICATION — JwtAuthStateProvider
// ═══════════════════════════════════════════════════════════
// On enregistre JwtAuthStateProvider comme AuthenticationStateProvider
// Blazor l'utilisera pour savoir si l'utilisateur est connecté
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthStateProvider>());

// Active les services d'autorisation (pour [Authorize] sur les pages)
builder.Services.AddAuthorizationCore();

// ═══════════════════════════════════════════════════════════
// SERVICES MÉTIER
// ═══════════════════════════════════════════════════════════
builder.Services.AddSingleton<LanguageService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<NotificationService>();

await builder.Build().RunAsync();
