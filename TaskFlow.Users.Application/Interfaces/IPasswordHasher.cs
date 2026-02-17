namespace TaskFlow.Users.Application.Interfaces;

/// <summary>
/// Abstraction pour le hachage de mots de passe.
/// L'interface est dans Application (couche haute) pour respecter la Dependency Inversion.
/// L'implémentation (BCrypt) est dans Infrastructure (couche basse).
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Transforme un mot de passe en clair en un hash irréversible.
    /// </summary>
    string Hash(string plainPassword);

    /// <summary>
    /// Vérifie si un mot de passe en clair correspond à un hash stocké.
    /// </summary>
    bool Verify(string plainPassword, string hashedPassword);
}
