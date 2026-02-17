namespace TaskFlow.Tasks.Domain.Enums;

/// <summary>
/// Niveaux de priorité d'une tâche.
/// 
/// POURQUOI un enum et pas un Value Object ?
/// Un enum est approprié quand :
/// - Les valeurs sont fixes et connues à l'avance
/// - Il n'y a pas de validation complexe
/// - C'est juste un choix parmi N options
/// 
/// Un Value Object est approprié quand :
/// - Il y a de la validation (ex: email, password)
/// - Il y a du comportement (ex: FullName.Complete)
/// </summary>
public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
