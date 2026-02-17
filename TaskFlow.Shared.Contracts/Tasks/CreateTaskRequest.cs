namespace TaskFlow.Shared.Contracts.Tasks;

public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    string Priority,
    DateTime? DueDate);
