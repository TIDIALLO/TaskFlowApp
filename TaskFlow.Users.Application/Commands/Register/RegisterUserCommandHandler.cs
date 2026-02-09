using MediatR;
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
    private readonly IMediator _mediator;  // 👈 Ajouter

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator)  // 👈 Ajouter
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;  // 👈 Ajouter
    }

    public async Task<Result<UserResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Créer les Value Objects
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result<UserResponse>.Failure(emailResult.Error);

        var passwordResult = Password.Create(request.Password);
        if (passwordResult.IsFailure)
            return Result<UserResponse>.Failure(passwordResult.Error);

        var fullNameResult = FullName.Create(request.FirstName, request.LastName);
        if (fullNameResult.IsFailure)
            return Result<UserResponse>.Failure(fullNameResult.Error);

        // 2. Vérifier si email existe
        if (await _userRepository.ExistsAsync(emailResult.Value, cancellationToken))
            return Result<UserResponse>.Failure(UserErrors.EmailAlreadyExists);

        // 3. Créer l'utilisateur
        var userResult = User.Create(emailResult.Value, passwordResult.Value, fullNameResult.Value);
        if (userResult.IsFailure)
            return Result<UserResponse>.Failure(userResult.Error);

        var user = userResult.Value;

        // 4. Sauvegarder
        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. 🔔 Publier la notification
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
