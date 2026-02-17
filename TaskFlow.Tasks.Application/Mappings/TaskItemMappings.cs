using TaskFlow.Shared.Contracts.Tasks;
using TaskFlow.Tasks.Domain.Entities;

namespace TaskFlow.Tasks.Application.Mappings;

/// <summary>
/// Méthodes d'extension pour mapper TaskItem → TaskItemResponse.
/// 
/// POURQUOI des extension methods pour le mapping ?
/// 1. DRY : le mapping Entity → DTO est fait à UN seul endroit
/// 2. Lisibilité : task.ToResponse() est plus clair que new TaskItemResponse(task.Id, ...)
/// 3. Maintenabilité : si on ajoute un champ, on le change ici et c'est propagé partout
/// 
/// Alternative : AutoMapper ou Mapster. Mais pour un projet de cette taille,
/// le mapping manuel est plus simple, plus rapide, et plus explicite.
/// Un senior .NET préfère souvent le mapping manuel pour sa transparence.
/// </summary>
public static class TaskItemMappings
{
    public static TaskItemResponse ToResponse(this TaskItem task) => new(
        task.Id,
        task.Title.Value,
        task.Description.Value,
        task.Priority.ToString(),        // Enum → string ("High", "Low"...)
        task.Status.ToString(),          // Enum → string ("Todo", "InProgress"...)
        task.DueDate,
        task.UserId,
        task.CreatedAt,
        task.CompletedAt);

    public static List<TaskItemResponse> ToResponseList(this List<TaskItem> tasks)
        => tasks.Select(t => t.ToResponse()).ToList();
}
