namespace TaskFlow.Shared.Contracts.Auth;

/// <summary>
/// POURQUOI un projet "Contracts" séparé ?
/// En Blazor WASM, le frontend et le backend sont en C#.
/// On peut donc PARTAGER les DTOs entre les deux.
/// Avantage : si tu ajoutes un champ côté API, le frontend ne compile plus
/// tant que tu ne l'as pas mis à jour → erreurs détectées à la COMPILATION.
/// Avec un frontend JS/React, tu ne détecterais l'erreur qu'au RUNTIME.
/// </summary>
public sealed record LoginRequest(string Email, string Password);
