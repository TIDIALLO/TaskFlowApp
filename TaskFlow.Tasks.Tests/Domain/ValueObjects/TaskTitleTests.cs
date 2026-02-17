using FluentAssertions;
using TaskFlow.Tasks.Domain.ValueObjects;
using TaskFlow.Tasks.Tests.Fixtures;

namespace TaskFlow.Tasks.Tests.Domain.ValueObjects;

/// <summary>
/// Tests unitaires pour le Value Object TaskTitle.
/// 
/// CONVENTION DE NOMMAGE : MethodName_Scenario_ExpectedBehavior
/// C'est la convention la plus répandue en .NET.
/// 
/// PATTERN AAA : Arrange → Act → Assert
/// Chaque test suit cette structure pour la lisibilité.
/// 
/// [Fact] = un test unique avec des données fixes.
/// [Theory] + [InlineData] = le même test exécuté avec PLUSIEURS jeux de données.
/// </summary>
public class TaskTitleTests
{
    [Fact]
    public void Create_WithValidTitle_ShouldReturnSuccess()
    {
        // Arrange
        var title = TaskFixtures.ValidTitle;

        // Act
        var result = TaskTitle.Create(title);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(title);
    }

    [Fact]
    public void Create_WithValidTitle_ShouldTrimWhitespace()
    {
        // Arrange — titre avec espaces autour
        var title = "  Ma tâche importante  ";

        // Act
        var result = TaskTitle.Create(title);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Ma tâche importante");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrNullTitle_ShouldReturnFailure(string? title)
    {
        // Act
        var result = TaskTitle.Create(title);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskTitle.Empty");
    }

    [Fact]
    public void Create_WithTooLongTitle_ShouldReturnFailure()
    {
        // Arrange — titre qui dépasse la limite de 200 caractères
        var title = TaskFixtures.TooLongTitle;

        // Act
        var result = TaskTitle.Create(title);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskTitle.TooLong");
    }

    [Fact]
    public void Create_WithExactMaxLength_ShouldReturnSuccess()
    {
        // Arrange — titre de EXACTEMENT 200 caractères (limite max)
        var title = new string('A', TaskTitle.MaxLength);

        // Act
        var result = TaskTitle.Create(title);

        // Assert — à la limite, ça passe
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void TwoTaskTitles_WithSameValue_ShouldBeEqual()
    {
        // Arrange & Act — deux Value Objects avec la même valeur
        var title1 = TaskTitle.Create("Même titre").Value;
        var title2 = TaskTitle.Create("Même titre").Value;

        // Assert — record = comparaison par VALEUR (pas par référence)
        title1.Should().Be(title2);
    }
}
