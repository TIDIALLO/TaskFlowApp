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
    private readonly IPasswordHasher _passwordHasher; // NOUVEAU : pour hasher le password
    private readonly IMediator _mediator;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IMediator mediator,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<UserResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to register user with email {Email}", request.Email);

        // 1. Créer les Value Objects (validation métier)
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            _logger.LogWarning("Invalid email format: {Email}", request.Email);
            return Result<UserResponse>.Failure(emailResult.Error);
        }

        var passwordResult = Password.Create(request.Password);
        if (passwordResult.IsFailure)
        {
            _logger.LogWarning("Invalid password for email: {Email}", request.Email);
            return Result<UserResponse>.Failure(passwordResult.Error);
        }

        var fullNameResult = FullName.Create(request.FirstName, request.LastName);
        if (fullNameResult.IsFailure)
        {
            _logger.LogWarning("Invalid name for email: {Email}", request.Email);
            return Result<UserResponse>.Failure(fullNameResult.Error);
        }

        // 2. Vérifier si email existe
        if (await _userRepository.ExistsAsync(emailResult.Value, cancellationToken))
        {
            _logger.LogWarning("Email already exists: {Email}", request.Email);
            return Result<UserResponse>.Failure(UserErrors.EmailAlreadyExists);
        }

        // 3. HASHER le mot de passe AVANT de créer l'entité
        //    Le password brut est dans passwordResult.Value.HashedValue (temporaire)
        //    On le hash avec BCrypt, puis on crée un Password "propre" avec le hash
        var hashedPassword = _passwordHasher.Hash(request.Password);
        var securePassword = Password.FromHash(hashedPassword);

        // 4. Créer l'utilisateur avec le password hashé
        var userResult = User.Create(emailResult.Value, securePassword, fullNameResult.Value);
        if (userResult.IsFailure)
            return Result<UserResponse>.Failure(userResult.Error);

        var user = userResult.Value;

        // 5. Sauvegarder en DB
        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} registered successfully", user.Id);

        // 6. Publier la notification (event-driven : les handlers réagissent sans couplage)
        await _mediator.Publish(
            new UserRegisteredNotification(user.Id, user.Email.Value, user.FullName.Complete),
            cancellationToken);

        // 7. Retourner le DTO (jamais l'entité directement !)
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
