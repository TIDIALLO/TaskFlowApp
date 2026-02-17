using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Users.Application.Commands.Login;
using TaskFlow.Users.Application.Commands.Register;
using TaskFlow.Users.Application.DTOs;
using TaskFlow.Users.Application.Queries.GetAllUsers;
using TaskFlow.Users.Application.Queries.GetUserById;

namespace TaskFlow.Api.Controllers;

public class UsersController : ApiController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register a new user.
    /// Retourne 201 Created avec le header Location pointant vers GetById.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        // HandleCreatedResult retourne 201 + header "Location: /api/users/{id}"
        // nameof(GetById) = le nom de la méthode GET qui permet de retrouver l'user
        // new { id = result.Value.Id } = les paramètres de route pour construire l'URL
        return HandleCreatedResult(
            result,
            nameof(GetById),
            new { id = result.IsSuccess ? result.Value.Id : Guid.Empty });
    }

    /// <summary>
    /// Login user. Retourne un JWT token.
    /// Pas de [Authorize] : on doit pouvoir se connecter sans être déjà connecté !
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)] // Corrigé : AuthResponse, pas UserResponse
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get user by ID. Requiert un token JWT valide.
    /// [Authorize] = si pas de token ou token invalide → 401 automatiquement.
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all users. Requiert un token JWT valide.
    /// </summary>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllUsersQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }
}
