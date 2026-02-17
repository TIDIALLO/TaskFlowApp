using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using TaskFlow.Users.Application.Commands.Register;
using TaskFlow.Users.Application.Interfaces;
using TaskFlow.Users.Domain.Entities;
using TaskFlow.Users.Domain.ValueObjects;
using TaskFlow.Users.Tests.Fixtures;

namespace TaskFlow.Users.Tests.Application.Commands;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock; // NOUVEAU
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<RegisterUserCommandHandler>> _loggerMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<RegisterUserCommandHandler>>();

        // Setup par défaut : le hasher retourne un hash prévisible
        _passwordHasherMock
            .Setup(x => x.Hash(It.IsAny<string>()))
            .Returns("$2a$12$hashedpassword");

        _handler = new RegisterUserCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object, // NOUVEAU paramètre
            _mediatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new RegisterUserCommand(
            UserFixtures.ValidEmail,
            UserFixtures.ValidPassword,
            UserFixtures.ValidFirstName,
            UserFixtures.ValidLastName);

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(UserFixtures.ValidEmail);
        result.Value.FirstName.Should().Be(UserFixtures.ValidFirstName);

        // Vérifie que le password a bien été hashé
        _passwordHasherMock.Verify(x => x.Hash(UserFixtures.ValidPassword), Times.Once);
        _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterUserCommand(
            UserFixtures.ValidEmail,
            UserFixtures.ValidPassword,
            UserFixtures.ValidFirstName,
            UserFixtures.ValidLastName);

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmailExists");

        _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterUserCommand(
            UserFixtures.InvalidEmail,
            UserFixtures.ValidPassword,
            UserFixtures.ValidFirstName,
            UserFixtures.ValidLastName);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.Invalid");
    }
}
