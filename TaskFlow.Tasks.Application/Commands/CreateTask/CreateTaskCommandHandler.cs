using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Shared.Contracts.Tasks;
using TaskFlow.Tasks.Application.Interfaces;
using TaskFlow.Tasks.Application.Mappings;
using TaskFlow.Tasks.Domain.Entities;
using TaskFlow.Tasks.Domain.Enums;
using TaskFlow.Tasks.Domain.ValueObjects;

namespace TaskFlow.Tasks.Application.Commands.CreateTask;

/// <summary>
/// Handler pour CreateTaskCommand.
/// 
/// PATTERN HANDLER :
/// Un handler a UNE seule responsabilité : traiter UNE commande.
/// C'est le Single Responsibility Principle (SRP — le S de SOLID).
/// 
/// LE FLUX :
/// 1. Controller reçoit la requête HTTP
/// 2. MediatR route la Command vers ce Handler
/// 3. Le Handler orchestre : validation Domain → création → persistance
/// 4. Le Handler retourne un Result au Controller
/// 
/// Le Handler est un ORCHESTRATEUR — il ne contient pas de logique métier.
/// La logique métier est dans l'ENTITÉ (TaskItem.Create, TaskTitle.Create...).
/// </summary>
public sealed class CreateTaskCommandHandler
    : IRequestHandler<CreateTaskCommand, Result<TaskItemResponse>>
{
    private readonly ITaskItemRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateTaskCommandHandler> _logger;

    public CreateTaskCommandHandler(
        ITaskItemRepository taskRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateTaskCommandHandler> logger)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TaskItemResponse>> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating task for user {UserId}", request.UserId);

        // 1. Créer les Value Objects (validation dans le Domain)
        var titleResult = TaskTitle.Create(request.Title);
        if (titleResult.IsFailure)
            return Result<TaskItemResponse>.Failure(titleResult.Error);

        var descriptionResult = TaskDescription.Create(request.Description);
        if (descriptionResult.IsFailure)
            return Result<TaskItemResponse>.Failure(descriptionResult.Error);

        // 2. Parser la priorité (string → enum)
        //    Enum.TryParse est sûr : si la string est invalide, il retourne false
        if (!Enum.TryParse<Priority>(request.Priority, ignoreCase: true, out var priority))
            return Result<TaskItemResponse>.Failure(new Error(
                "TaskItem.InvalidPriority",
                "Invalid priority value.",
                ErrorType.Validation));

        // 3. Créer l'entité via la factory method (qui applique les règles métier)
        var taskResult = TaskItem.Create(
            titleResult.Value,
            descriptionResult.Value,
            priority,
            request.DueDate,
            request.UserId);

        if (taskResult.IsFailure)
            return Result<TaskItemResponse>.Failure(taskResult.Error);

        var task = taskResult.Value;

        // 4. Persister
        _taskRepository.Add(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Task {TaskId} created for user {UserId}", task.Id, task.UserId);

        // 5. Mapper vers DTO et retourner
        return Result<TaskItemResponse>.Success(task.ToResponse());
    }
}
