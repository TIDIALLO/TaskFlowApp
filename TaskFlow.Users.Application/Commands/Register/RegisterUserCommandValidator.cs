using FluentValidation;

namespace TaskFlow.Users.Application.Commands.Register;

/// <summary>
/// Validateur FluentValidation pour RegisterUserCommand.
/// 
/// POURQUOI séparer la validation du handler ?
/// 1. Single Responsibility : le handler fait la logique métier, le validateur valide les données
/// 2. Réutilisabilité : le même validateur peut être utilisé côté client ou dans les tests
/// 3. Automatisation : grâce au Pipeline Behavior, PAS besoin d'appeler le validateur manuellement
/// 
/// QUAND utiliser FluentValidation vs Value Objects ?
/// - FluentValidation = validation de "surface" (champs requis, formats basiques)
/// - Value Objects = validation de "domaine" (règles métier complexes)
/// Les deux se complètent : FluentValidation rejette vite les requêtes mal formées
/// AVANT même qu'on touche au Domain.
/// </summary>
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        // RuleFor(x => x.Prop) définit une règle de validation sur une propriété
        // .NotEmpty() = ne doit pas être null, vide, ou que des espaces
        // .WithMessage() = message d'erreur personnalisé

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");
    }
}
