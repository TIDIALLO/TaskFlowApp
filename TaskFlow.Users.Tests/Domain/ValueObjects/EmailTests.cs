using FluentAssertions;
using TaskFlow.Users.Domain.ValueObjects;
using TaskFlow.Users.Tests.Fixtures;

namespace TaskFlow.Users.Tests.Domain.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldReturnSuccess()
    {
        // Arrange
        var emailAddress = UserFixtures.ValidEmail;

        // Act
        var result = Email.Create(emailAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(emailAddress);
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var emailAddress = UserFixtures.InvalidEmail;

        // Act
        var result = Email.Create(emailAddress);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.Invalid");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ShouldReturnFailure(string? emailAddress)
    {
        // Act
        var result = Email.Create(emailAddress);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.Empty");
    }

    [Fact]
    public void Create_ShouldNormalizeToLowercase()
    {
        // Arrange
        var emailAddress = "TEST@EXAMPLE.COM";

        // Act
        var result = Email.Create(emailAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("test@example.com");
    }
}
