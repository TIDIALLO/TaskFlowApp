using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Notifications.Application.Commands.MarkAllAsRead;
using TaskFlow.Notifications.Application.Commands.MarkAsRead;
using TaskFlow.Notifications.Application.Queries.GetUnreadCount;
using TaskFlow.Notifications.Application.Queries.GetUserNotifications;

namespace TaskFlow.Api.Controllers;

/// <summary>
/// Controller pour les notifications.
/// Même pattern que TasksController : [Authorize] + extraction du UserId du JWT.
/// 
/// ENDPOINTS :
/// GET    /api/notifications          → liste des notifications de l'utilisateur
/// GET    /api/notifications/unread   → nombre de non-lues (pour le badge)
/// PATCH  /api/notifications/{id}/read → marquer une notification comme lue
/// PATCH  /api/notifications/read-all  → tout marquer comme lu
/// </summary>
[Authorize]
public class NotificationsController : ApiController
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Extrait le UserId du JWT (claim "sub" ou "nameid").
    /// </summary>
    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub");
        return Guid.Parse(claim!.Value);
    }

    /// <summary>
    /// GET /api/notifications — toutes les notifications de l'utilisateur connecté
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = GetUserId();
        var query = new GetUserNotificationsQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/notifications/unread — nombre de non-lues
    /// Appelé en polling par le frontend pour mettre à jour le badge
    /// </summary>
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        var query = new GetUnreadCountQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// PATCH /api/notifications/{id}/read — marquer UNE notification comme lue
    /// </summary>
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetUserId();
        var command = new MarkAsReadCommand(id, userId);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// PATCH /api/notifications/read-all — tout marquer comme lu
    /// </summary>
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetUserId();
        var command = new MarkAllAsReadCommand(userId);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}
