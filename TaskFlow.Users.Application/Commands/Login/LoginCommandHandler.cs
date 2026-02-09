using MediatR;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Users.Application.DTOs;
using TaskFlow.Users.Application.Interfaces;
using TaskFlow.Users.Domain.Errors;
using TaskFlow.Users.Domain.ValueObjects;

namespace TaskFlow.Users.Application.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Valider l'email
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result<AuthResponse>.Failure(UserErrors.InvalidCredentials);

        // 2. Chercher l'utilisateur
        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null)
            return Result<AuthResponse>.Failure(UserErrors.InvalidCredentials);

        // 3. Vérifier le password
        if (user.Password.HashedValue != request.Password)
            return Result<AuthResponse>.Failure(UserErrors.InvalidCredentials);

        // 4. Vérifier si actif
        if (!user.IsActive)
            return Result<AuthResponse>.Failure(UserErrors.Inactive);

        // 5. Générer le token JWT
        var token = _jwtService.GenerateToken(user);

        // 6. Retourner la réponse avec le token
        var response = new AuthResponse(
            user.Id,
            user.Email.Value,
            user.FullName.Complete,
            token);

        return Result<AuthResponse>.Success(response);
    }
}