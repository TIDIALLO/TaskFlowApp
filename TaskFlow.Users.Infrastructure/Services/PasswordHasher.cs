using TaskFlow.Users.Application.Interfaces;

namespace TaskFlow.Users.Infrastructure.Services;

/// <summary>
/// Implémentation du hachage avec BCrypt.
/// 
/// BCrypt est un algorithme conçu spécifiquement pour les mots de passe :
/// - Il est LENT exprès (pour empêcher les attaques brute-force)
/// - Il inclut un "salt" automatique (deux mêmes mots de passe donnent des hashs différents)
/// - Le "work factor" (ici 11) contrôle la lenteur : chaque +1 double le temps de calcul
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    // Work factor = 11 → bon compromis sécurité/performance (~200ms par hash)
    private const int WorkFactor = 11;

    public string Hash(string plainPassword)
    {
        // BCrypt génère automatiquement un salt aléatoire et le combine avec le mot de passe
        // Résultat : "$2a$11$rK7gZ..." — contient l'algo, le work factor, le salt, et le hash
        return BCrypt.Net.BCrypt.HashPassword(plainPassword, WorkFactor);
    }

    public bool Verify(string plainPassword, string hashedPassword)
    {
        // BCrypt extrait le salt du hash stocké, re-hashe le mot de passe entré
        // et compare les deux résultats
        return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
    }
}
