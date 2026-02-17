using FluentAssertions;
using TaskFlow.Tasks.Domain.ValueObjects;
using TaskFlow.Tasks.Tests.Fixtures;

namespace TaskFlow.Tasks.Tests.Domain.ValueObjects;

/// <summary>
/// Tests unitaires pour le Value Object TaskDescription.
/// 
/// PARTICULARITÉ : contrairement à TaskTitle, la description est OPTIONNELLE.
/// null ou "" → pas d'erreur, juste une description vide.
/// Seule la longueur max est validée.
/// </summary>
public class TaskDescriptionTests
{
    [Fact]
    public void Create_WithValidDescription_ShouldReturnSuccess()
    {
        // Arrange
        var description = TaskFixtures.ValidDescription;

        // Act
        var result = TaskDescription.Create(description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmpty_ShouldReturnSuccessWithEmptyValue(string? description)
    {
        // Act — null/vide est ACCEPTÉ (la description est optionnelle)
        var result = TaskDescription.Create(description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithTooLongDescription_ShouldReturnFailure()
    {
        // Arrange — description qui dépasse 2000 caractères
        var description = TaskFixtures.TooLongDescription;

        // Act
        var result = TaskDescription.Create(description);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskDescription.TooLong");
    }

    [Fact]
    public void Create_WithExactMaxLength_ShouldReturnSuccess()
    {
        // Arrange
        var description = new string('X', TaskDescription.MaxLength);

        // Act
        var result = TaskDescription.Create(description);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Empty_ShouldReturnDescriptionWithEmptyValue()
    {
        // Act — propriété statique de raccourci
        var empty = TaskDescription.Empty;

        // Assert
        empty.Value.Should().BeEmpty();
    }
}
