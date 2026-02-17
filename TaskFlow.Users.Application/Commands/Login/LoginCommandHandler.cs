using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Users.Application.DTOs;
using TaskFlow.Users.Application.Interfaces;
using TaskFlow.Users.Domain.Errors;
using TaskFlow.Users.Domain.ValueObjects;

namespace TaskFlow.Users.Application.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for {Email}", request.Email);

        // 1. Valider le format de l'email
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result<AuthResponse>.Failure(UserErrors.InvalidCredentials);

        // 2. Chercher l'utilisateur par email
        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null)
        {
            // SÉCURITÉ : on ne dit pas "email non trouvé" pour ne pas révéler
            // quels emails sont enregistrés (prévention d'énumération)
            _logger.LogWarning("Login failed: user not found for {Email}", request.Email);
            return Result<AuthResponse>.Failure(UserErrors.InvalidCredentials);
        }

        // 3. Vérifier le mot de passe avec BCrypt
        //    BCrypt.Verify compare le mot de passe brut avec le hash stocké en DB
        if (!_passwordHasher.Verify(request.Password, user.Password.HashedValue))
        {
            _logger.LogWarning("Login failed: invalid password for {Email}", request.Email);
            return Result<AuthResponse>.Failure(UserErrors.InvalidCredentials);
        }

        // 4. Vérifier si le compte est actif
        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed: inactive account {Email}", request.Email);
            return Result<AuthResponse>.Failure(UserErrors.Inactive);
        }

        // 5. Générer le token JWT
        var token = _jwtService.GenerateToken(user);

        _logger.LogInformation("Login successful for {UserId}", user.Id);

        // 6. Retourner la réponse avec le token
        var response = new AuthResponse(
            user.Id,
            user.Email.Value,
            user.FullName.Complete,
            token);

        return Result<AuthResponse>.Success(response);
    }
}
