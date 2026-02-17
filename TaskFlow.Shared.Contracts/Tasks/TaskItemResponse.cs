namespace TaskFlow.Shared.Contracts.Tasks;

public sealed record TaskItemResponse(
    Guid Id,
    string Title,
    string Description,
    string Priority,
    string Status,
    DateTime? DueDate,
    Guid UserId,
    DateTime CreatedAt,
    DateTime? CompletedAt);
