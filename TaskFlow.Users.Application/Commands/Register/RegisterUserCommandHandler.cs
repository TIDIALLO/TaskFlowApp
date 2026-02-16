using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Users.Application.DTOs;
using TaskFlow.Users.Application.Interfaces;
using TaskFlow.Users.Application.Notifications;
using TaskFlow.Users.Domain.Entities;
using TaskFlow.Users.Domain.Errors;
using TaskFlow.Users.Domain.ValueObjects;

namespace TaskFlow.Users.Application.Commands.Register;

public sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, Result<UserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<UserResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("📝 Attempting to register user with email {Email}", request.Email);

        // 1. Créer les Value Objects
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            _logger.LogWarning("❌ Invalid email format: {Email}", request.Email);
            return Result<UserResponse>.Failure(emailResult.Error);
        }

        var passwordResult = Password.Create(request.Password);
        if (passwordResult.IsFailure)
        {
            _logger.LogWarning("❌ Invalid password for email: {Email}", request.Email);
            return Result<UserResponse>.Failure(passwordResult.Error);
        }

        var fullNameResult = FullName.Create(request.FirstName, request.LastName);
        if (fullNameResult.IsFailure)
        {
            _logger.LogWarning("❌ Invalid name for email: {Email}", request.Email);
            return Result<UserResponse>.Failure(fullNameResult.Error);
        }

        // 2. Vérifier si email existe
        if (await _userRepository.ExistsAsync(emailResult.Value, cancellationToken))
        {
            _logger.LogWarning("❌ Email already exists: {Email}", request.Email);
            return Result<UserResponse>.Failure(UserErrors.EmailAlreadyExists);
        }

        // 3. Créer l'utilisateur
        var userResult = User.Create(emailResult.Value, passwordResult.Value, fullNameResult.Value);
        if (userResult.IsFailure)
            return Result<UserResponse>.Failure(userResult.Error);

        var user = userResult.Value;

        // 4. Sauvegarder
        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("✅ User {UserId} saved to database", user.Id);

        // 5. Publier la notification
        await _mediator.Publish(
            new UserRegisteredNotification(
                user.Id,
                user.Email.Value,
                user.FullName.Complete),
            cancellationToken);

        // 6. Retourner le DTO
        var response = new UserResponse(
            user.Id,
            user.Email.Value,
            user.FullName.FirstName,
            user.FullName.LastName,
            user.IsActive,
            user.CreatedAt);

        return Result<UserResponse>.Success(response);
    }
}
