using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Shared.Contracts.Tasks;
using TaskFlow.Tasks.Application.Commands.ChangeTaskStatus;
using TaskFlow.Tasks.Application.Commands.CreateTask;
using TaskFlow.Tasks.Application.Commands.DeleteTask;
using TaskFlow.Tasks.Application.Commands.UpdateTask;
using TaskItemResponse = TaskFlow.Shared.Contracts.Tasks.TaskItemResponse;
using TaskFlow.Tasks.Application.Queries.GetTaskById;
using TaskFlow.Tasks.Application.Queries.GetUserTasks;

namespace TaskFlow.Api.Controllers;

/// <summary>
/// Contrôleur pour les opérations CRUD sur les tâches.
/// 
/// [Authorize] sur la CLASSE entière = TOUS les endpoints nécessitent un JWT valide.
/// Contrairement à UsersController où Register et Login sont publics.
/// 
/// COMMENT on récupère le UserId ?
/// Quand le JWT est validé par le middleware Authentication, les claims du token
/// sont injectés dans HttpContext.User. On extrait le claim NameIdentifier
/// (qu'on a mis dans le token lors du login, voir JwtService).
/// </summary>
[Authorize]
public class TasksController : ApiController
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Extrait le UserId depuis le token JWT.
    /// Le claim ClaimTypes.NameIdentifier contient le GUID de l'utilisateur.
    /// On l'a mis dans le token dans JwtService.GenerateToken().
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// POST /api/tasks — Créer une nouvelle tâche.
    /// Le UserId vient du token JWT, pas du body (sécurité).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        // On crée la command avec le UserId extrait du JWT
        // Le client n'envoie PAS son userId dans le body — c'est le token qui fait foi
        var command = new CreateTaskCommand(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            GetCurrentUserId());

        var result = await _mediator.Send(command, cancellationToken);

        return HandleCreatedResult(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value.Id : Guid.Empty });
    }

    /// <summary>
    /// GET /api/tasks/{id} — Récupérer une tâche par ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetTaskByIdQuery(id, GetCurrentUserId());
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// GET /api/tasks — Récupérer toutes les tâches de l'utilisateur connecté.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TaskItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTasks(CancellationToken cancellationToken)
    {
        var query = new GetUserTasksQuery(GetCurrentUserId());
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// PUT /api/tasks/{id} — Mettre à jour une tâche.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTaskCommand(
            id,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            GetCurrentUserId());

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// PATCH /api/tasks/{id}/status — Changer le statut d'une tâche.
    /// 
    /// POURQUOI PATCH et pas PUT ?
    /// - PUT = remplacer la ressource ENTIÈRE
    /// - PATCH = modifier PARTIELLEMENT la ressource
    /// Ici on ne change que le statut → PATCH est sémantiquement correct.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TaskItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] ChangeStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeTaskStatusCommand(id, request.NewStatus, GetCurrentUserId());
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// DELETE /api/tasks/{id} — Supprimer une tâche.
    /// Retourne 204 No Content (pas de body).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteTaskCommand(id, GetCurrentUserId());
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
