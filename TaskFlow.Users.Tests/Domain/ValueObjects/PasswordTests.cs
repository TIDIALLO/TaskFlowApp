using FluentAssertions;
using TaskFlow.Users.Domain.ValueObjects;
using TaskFlow.Users.Tests.Fixtures;

namespace TaskFlow.Users.Tests.Domain.ValueObjects;

public class PasswordTests
{
    [Fact]
    public void Create_WithValidPassword_ShouldReturnSuccess()
    {
        // Arrange
        var password = UserFixtures.ValidPassword;

        // Act
        var result = Password.Create(password);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithShortPassword_ShouldReturnFailure()
    {
        // Arrange
        var password = UserFixtures.ShortPassword;

        // Act
        var result = Password.Create(password);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Password.TooShort");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyPassword_ShouldReturnFailure(string? password)
    {
        // Act
        var result = Password.Create(password);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Password.Empty");
    }
}
