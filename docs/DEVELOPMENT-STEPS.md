# 🛠️ TaskFlow — Étapes de développement (pas à pas)

Ce document décrit **dans l'ordre** chaque étape du développement de TaskFlow, avec les concepts appris à chaque phase.

---

## Phase 1 : Fondations (Shared Kernel)

### Étape 1.1 — Créer la solution et les projets

```bash
dotnet new sln -n TaskFlow
dotnet new classlib -n TaskFlow.Shared.Kernel
dotnet sln add TaskFlow.Shared.Kernel
```

### Étape 1.2 — Entity de base

**Fichier** : `TaskFlow.Shared.Kernel/Primitives/Entity.cs`

- Classe abstraite avec `Guid Id`
- Implémente `IEquatable<Entity>` (comparaison par Id)
- Liste de `IDomainEvent` pour stocker les events avant dispatch
- Tous les objets du domaine héritent de cette classe

**Concept appris** : toute entité DDD est identifiée par un **Id unique**, pas par ses propriétés.

### Étape 1.3 — Result Pattern

**Fichiers** : `Result.cs`, `Error.cs`

- `Result` = succès ou échec (sans exception)
- `Result<T>` = succès avec une valeur, ou échec avec une erreur
- `Error` = code + message + type (Validation, NotFound, Conflict, etc.)

**Concept appris** : on ne throw pas d'exceptions pour le flux métier. Le `Result` rend l'erreur **visible dans le type de retour**.

### Étape 1.4 — IDomainEvent (Marker Interface)

**Fichier** : `TaskFlow.Shared.Kernel/Primitives/IDomainEvent.cs`

```csharp
public interface IDomainEvent : INotification { }
```

- Hérite de `MediatR.INotification` pour le Pub/Sub
- Placé dans Shared.Kernel pour être accessible par TOUS les modules
- Permet la communication cross-module sans couplage

**Concept appris** : un marker interface définit un contrat commun sans ajouter de méthode.

---

## Phase 2 : Module Users — Domain

### Étape 2.1 — Value Objects

**Fichiers** : `Email.cs`, `Password.cs`, `FullName.cs`

- Chacun a un constructeur privé + `Create()` qui valide
- `Email.Create("bad")` → `Result.Failure(...)`
- `Email.Create("user@mail.com")` → `Result.Success(email)`

**Concept appris** : un Value Object garantit qu'une valeur est **toujours valide**. On ne peut pas créer un `Email` invalide.

### Étape 2.2 — Entité User

**Fichier** : `User.cs`

- Propriétés avec `private set` (encapsulation)
- Factory method `User.Create(email, password, fullName)`
- Méthodes métier : `ChangeEmail()`, `Deactivate()`

**Concept appris** : Rich Domain Model — l'entité porte les **règles métier**, pas les services.

### Étape 2.3 — Erreurs domaine

**Fichier** : `UserErrors.cs`

- Erreurs nommées : `UserErrors.NotFound`, `UserErrors.EmailTaken`, etc.
- Chaque erreur a un `ErrorType` qui sera mappé vers un code HTTP

---

## Phase 3 : Module Users — Application (CQRS)

### Étape 3.1 — Interfaces

**Fichiers** : `IUserRepository.cs`, `IJwtService.cs`, `IPasswordHasher.cs`, `IUnitOfWork.cs`

- Contrats abstraits : l'Application dit **ce dont elle a besoin** sans savoir **comment c'est implémenté**

**Concept appris** : Dependency Inversion Principle (le D de SOLID).

### Étape 3.2 — Commands (écriture)

**RegisterUserCommand** : inscription d'un utilisateur
- Command = record immutable
- Handler = orchestre : valide → crée → persiste → publie UserRegisteredNotification
- Validator (FluentValidation) = vérifie les données entrantes

**LoginCommand** : connexion
- Vérifie email → password BCrypt → génère JWT

### Étape 3.3 — Queries (lecture)

**GetUserByIdQuery**, **GetAllUsersQuery**

**Concept appris** : CQRS — séparer lecture et écriture pour la clarté.

### Étape 3.4 — ValidationBehavior (pipeline MediatR)

**Fichier** : `ValidationBehavior.cs`

- S'intercale automatiquement avant chaque Handler
- Exécute les Validators FluentValidation
- Si invalide → retourne `Result.Failure` sans appeler le Handler

**Concept appris** : MediatR Pipeline Behaviors = middleware au niveau application.

### Étape 3.5 — UserRegisteredNotification

**Fichier** : `Notifications/UserRegisteredNotification.cs`

```csharp
public record UserRegisteredNotification(Guid UserId, string FullName) : INotification;
```

- Publié par `RegisterUserCommandHandler` après la création de l'utilisateur
- Sera écouté par le module Notifications pour créer un message de bienvenue

**Concept appris** : MediatR Notification = event applicatif publié manuellement dans un Handler.

---

## Phase 4 : Module Users — Infrastructure

### Étape 4.1 — EF Core DbContext

**Fichier** : `UsersDbContext.cs`

- `DbSet<User>` pour la table Users
- `ApplyConfigurationsFromAssembly` pour charger les Fluent API configs

### Étape 4.2 — Configurations EF Core

**Fichier** : `UserConfiguration.cs`

- Mappe les Value Objects vers des colonnes SQL
- Définit les contraintes (taille, index unique sur Email)
- **Ignore** la propriété `DomainEvents` (non mappée en DB)

### Étape 4.3 — Repositories et Services

- `UserRepository` : implémente `IUserRepository` avec EF Core
- `JwtService` : génère des JWT avec les claims de l'utilisateur
- `PasswordHasher` : BCrypt pour hasher et vérifier les mots de passe
- `UnitOfWork` : encapsule `SaveChangesAsync`

### Étape 4.4 — DependencyInjection.cs

Extension method `AddUsersInfrastructure()` qui enregistre tout dans le DI container.

### Étape 4.5 — Migrations

```bash
dotnet ef migrations add InitialCreate --project TaskFlow.Users.Infrastructure --startup-project TaskFlow.Api
dotnet ef database update --project TaskFlow.Users.Infrastructure --startup-project TaskFlow.Api
```

---

## Phase 5 : API

### Étape 5.1 — Programme.cs

Configuration dans l'ordre :
1. Serilog (logging)
2. Services (modules via extension methods)
3. JWT Authentication
4. CORS (pour le frontend Blazor)
5. Swagger
6. Pipeline : ExceptionHandler → Serilog → CORS → Auth → Authorization → Controllers

### Étape 5.2 — ApiController (base)

Centralise la conversion `Result<T>` → `IActionResult` :
- Success → 200 OK ou 201 Created
- Failure → 400/401/403/404/409 selon `ErrorType`

### Étape 5.3 — UsersController

- `POST /register` — pas de [Authorize]
- `POST /login` — pas de [Authorize]
- `GET /{id}` — [Authorize]
- `GET /` — [Authorize]

### Étape 5.4 — GlobalExceptionHandler

Catch-all pour les exceptions non gérées → retourne ProblemDetails (RFC 7807).

---

## Phase 6 : Module Tasks (même structure que Users)

### Étape 6.1 — Domain

- `TaskItem` avec cycle de vie : `Start()`, `Complete()`, `Cancel()`
- Value Objects : `TaskTitle`, `TaskDescription`
- Enums : `Priority`, `TaskItemStatus`
- **Domain Events** : `TaskCreatedEvent`, `TaskCompletedEvent`, `TaskStatusChangedEvent`

**Nouveau concept** : les entités lèvent des **Domain Events** sans connaître les listeners.

```csharp
public static Result<TaskItem> Create(...)
{
    var task = new TaskItem(...);
    task.AddDomainEvent(new TaskCreatedEvent(task.Id, userId, title.Value));
    return Result.Success(task);
}

public Result Complete()
{
    Status = TaskItemStatus.Done;
    AddDomainEvent(new TaskCompletedEvent(Id, UserId, Title.Value));
    return Result.Success();
}
```

### Étape 6.2 — Application

- Commands : Create, Update, ChangeStatus, Delete
- Queries : GetById, GetUserTasks
- Validators FluentValidation pour chaque Command

### Étape 6.3 — Infrastructure

- `TasksDbContext` séparé (modular monolith)
- `TaskItemRepository`
- **UnitOfWork avec dispatch d'events** :

```csharp
public async Task<int> SaveChangesAsync(CancellationToken ct)
{
    var result = await _context.SaveChangesAsync(ct);  // 1. Persister
    
    var entities = _context.ChangeTracker
        .Entries<Entity>()
        .Where(e => e.Entity.DomainEvents.Any());
    
    foreach (var entity in entities)
    {
        foreach (var domainEvent in entity.Entity.DomainEvents)
            await _mediator.Publish(domainEvent, ct);  // 2. Publier
        entity.Entity.ClearDomainEvents();              // 3. Nettoyer
    }
    return result;
}
```

**Concept appris** : Dispatch After SaveChanges — les events sont publiés APRÈS la persistance pour garantir la cohérence des données.

### Étape 6.4 — TasksController

- Tous les endpoints sont `[Authorize]`
- Le `UserId` est extrait du JWT (pas envoyé par le client)

---

## Phase 7 : Shared Contracts

### Étape 7.1 — Créer le projet

```bash
dotnet new classlib -n TaskFlow.Shared.Contracts
```

### Étape 7.2 — DTOs partagés

Les `Request` et `Response` records sont dans Shared.Contracts :
- Le Client ET l'API référencent ce projet
- Si un champ change, les deux côtés doivent compiler
- Erreurs détectées à la **compilation**, pas au runtime
- Inclut `Auth/`, `Tasks/`, et `Notifications/`

---

## Phase 8 : Frontend Blazor WASM

### Étape 8.1 — Setup

```bash
dotnet new blazorwasm -n TaskFlow.Client
```

- Configure HttpClient pointant vers l'API
- Ajoute Blazored.LocalStorage pour stocker le JWT
- Enregistre AuthService, TaskService, NotificationService, LanguageService dans le DI

### Étape 8.2 — JwtAuthStateProvider

Implémente `AuthenticationStateProvider` :
- Lit le JWT depuis localStorage
- Décode les claims sans appel serveur
- Notifie Blazor quand l'état d'auth change

### Étape 8.3 — Pages

- **Home.razor** : landing page professionnelle avec hero section, features grid, CTA
- **Login.razor** : formulaire avec card, gradient header, icons
- **Register.razor** : formulaire d'inscription avec design pro
- **Tasks.razor** : dashboard avec filtres tabs, création, changement de status, toast notifications
- **Notifications.razor** : centre de notifications avec icons par type, mark as read

### Étape 8.4 — Services Client

- `AuthService` : Login → stocke JWT → notifie Blazor
- `TaskService` : CRUD via HttpClient avec Bearer token automatique
- `NotificationService` : polling des notifications, mark as read
- `LanguageService` : gestion FR/EN (singleton avec event)

---

## Phase 9 : Module Notifications (Communication inter-modules)

### Étape 9.1 — Domain

**Fichier** : `Notification.cs`

- Entity avec `UserId`, `Type`, `Title`, `Message`, `IsRead`, `CreatedAt`
- Factory method `Notification.Create(userId, type, title, message)`
- Méthode `MarkAsRead()` (idempotente)
- `NotificationType` enum : `Welcome`, `TaskCreated`, `TaskCompleted`, `StatusChanged`

### Étape 9.2 — Application (CQRS + EventHandlers)

**Commands** :
- `MarkAsReadCommand` : marquer une notification lue (vérifie userId = owner)
- `MarkAllAsReadCommand` : marquer toutes les notifs de l'user comme lues

**Queries** :
- `GetUserNotificationsQuery` : liste les notifications de l'user (triées par date desc)
- `GetUnreadCountQuery` : compte les non-lues

**⚡ EventHandlers (Cross-Module)** :

```csharp
// Écoute un event du module Users
public class OnUserRegistered_CreateWelcomeNotification 
    : INotificationHandler<UserRegisteredNotification>
{
    public async Task Handle(UserRegisteredNotification notification, CancellationToken ct)
    {
        var notif = Notification.Create(
            notification.UserId,
            NotificationType.Welcome,
            "Bienvenue !",
            $"Bienvenue {notification.FullName} sur TaskFlow !");
        await _repository.AddAsync(notif);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

// Écoute un event du module Tasks
public class OnTaskCreated_NotifyUser 
    : INotificationHandler<TaskCreatedEvent> { ... }

public class OnTaskCompleted_CongratulateUser 
    : INotificationHandler<TaskCompletedEvent> { ... }
```

**Concept clé** : le module Notifications ne référence **pas** l'infrastructure des autres modules. Il écoute uniquement leurs events (définis dans Domain ou Application).

### Étape 9.3 — Infrastructure

- `NotificationsDbContext` : DbContext isolé avec `DbSet<Notification>`
- `NotificationRepository` : CRUD via EF Core
- `NotificationUnitOfWork` : simple SaveChanges (pas de dispatch d'events ici)
- `DependencyInjection.cs` : `AddNotificationsInfrastructure()`

### Étape 9.4 — NotificationsController

```csharp
[Authorize]
[Route("api/notifications")]
public class NotificationsController : ApiController
{
    GET  /                  → GetUserNotificationsQuery
    GET  /unread-count      → GetUnreadCountQuery
    PATCH /{id}/read        → MarkAsReadCommand
    PATCH /read-all         → MarkAllAsReadCommand
}
```

### Étape 9.5 — Intégration dans Program.cs

```csharp
builder.Services.AddNotificationsApplication();
builder.Services.AddNotificationsInfrastructure(builder.Configuration);
```

**Concepts appris** :
- **Pub/Sub** : un module publie, un autre écoute, sans couplage
- **Domain Events** : l'entité publie des events sans connaître les listeners
- **Cross-Module Event Handler** : un handler dans un module écoute les events d'un autre
- **DbContext isolé par module** : Shared Database, Isolated Contexts

---

## Phase 10 : UI/UX Professionnelle

### Étape 10.1 — Enrichir Bootstrap avec un thème custom

**Fichier** : `wwwroot/css/app.css`

- CSS variables custom (`--primary`, `--primary-gradient`, etc.)
- Palette : Indigo (#4F46E5) + Slate (#1E293B)
- Typographie : Inter (Google Fonts)
- Animations : fadeIn, slideUp pour les transitions
- Cards avec box-shadow et border-radius améliorés
- Boutons avec gradients et hover effects

**Principe** : enrichir Bootstrap, pas le remplacer — on profite de la solidité de Bootstrap en ajoutant de la personnalisation.

### Étape 10.2 — Home page landing

- Hero section avec gradient et CTA
- Features grid (3 colonnes) avec icônes
- Section statistiques
- Call-to-action final

### Étape 10.3 — Amélioration de toutes les pages

- Login/Register : cards avec headers gradient, icônes dans les inputs
- Tasks : tabs filtrés, bords couleur par priorité, toasts animés
- Notifications : icônes par type, badge pulsant, formatage de dates
- NavMenu : logo gradient, badge de notifications, bouton langue

### Étape 10.4 — Responsive design

- Mobile-first via Bootstrap grid
- Sidebar adaptative
- Touch-friendly buttons

---

## Phase 11 : Internationalisation (I18n)

### Étape 11.1 — LanguageService

**Fichier** : `Services/LanguageService.cs`

```csharp
public class LanguageService
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    
    public string CurrentLanguage { get; private set; } = "FR";
    public event Action? OnLanguageChanged;
    
    public void SetLanguage(string lang)
    {
        CurrentLanguage = lang;
        OnLanguageChanged?.Invoke();
    }
    
    public string T(string key) => _translations[CurrentLanguage].GetValueOrDefault(key, key);
}
```

### Étape 11.2 — Intégration dans les composants

Chaque page Razor :
1. Injecte `@inject LanguageService Lang`
2. S'abonne à `Lang.OnLanguageChanged += StateHasChanged`
3. Utilise `@Lang.T("key")` pour chaque texte
4. Se désabonne dans `Dispose()`

### Étape 11.3 — Toggle de langue dans NavMenu

Bouton FR/EN dans la navbar qui appelle `Lang.SetLanguage()`.
L'interface se re-rend instantanément dans la nouvelle langue.

**Concept appris** : I18n côté client avec un singleton, événement de changement, et re-render Blazor.

---

## Phase 12 : Tests unitaires complets

### Étape 12.1 — Structure des projets de test

```bash
dotnet new xunit -n TaskFlow.Tasks.Tests
dotnet new xunit -n TaskFlow.Notifications.Tests
dotnet sln add TaskFlow.Tasks.Tests
dotnet sln add TaskFlow.Notifications.Tests
```

Packages : `xUnit`, `Moq`, `FluentAssertions`

### Étape 12.2 — Fixtures (données de test réutilisables)

```csharp
public static class TaskFixtures
{
    public static readonly Guid ValidUserId = Guid.NewGuid();
    
    public static TaskItem CreateValidTask() =>
        TaskItem.Create(
            TaskTitle.Create("Test Task").Value,
            TaskDescription.Create("Description").Value,
            Priority.Medium,
            DateTime.UtcNow.AddDays(7),
            ValidUserId).Value;
}
```

### Étape 12.3 — Tests Domain (Value Objects)

```csharp
[Fact]
public void Create_WithValidTitle_ShouldSucceed()
{
    // Arrange
    var title = "Ma tâche";
    // Act
    var result = TaskTitle.Create(title);
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Value.Should().Be(title);
}

[Theory]
[InlineData("")]
[InlineData(null)]
[InlineData("   ")]
public void Create_WithInvalidTitle_ShouldFail(string? title)
{
    var result = TaskTitle.Create(title!);
    result.IsFailure.Should().BeTrue();
}
```

**Concept appris** : `[Fact]` pour un cas unique, `[Theory]` + `[InlineData]` pour des cas paramétrés.

### Étape 12.4 — Tests Domain (Entities)

```csharp
[Fact]
public void Complete_FromInProgress_ShouldSucceed_And_RaiseDomainEvent()
{
    var task = TaskFixtures.CreateValidTask();
    task.Start(); // Todo → InProgress
    
    var result = task.Complete(); // InProgress → Done
    
    result.IsSuccess.Should().BeTrue();
    task.Status.Should().Be(TaskItemStatus.Done);
    task.DomainEvents.Should().ContainSingle(e => e is TaskCompletedEvent);
}
```

### Étape 12.5 — Tests Application (Command Handlers avec Mocks)

```csharp
[Fact]
public async Task Handle_ValidCommand_ShouldCreateTask()
{
    // Arrange
    var mockRepo = new Mock<ITaskItemRepository>();
    var mockUow = new Mock<IUnitOfWork>();
    TaskItem? capturedTask = null;
    mockRepo.Setup(r => r.AddAsync(It.IsAny<TaskItem>()))
            .Callback<TaskItem>(t => capturedTask = t)
            .Returns(Task.CompletedTask);
    
    var handler = new CreateTaskCommandHandler(mockRepo.Object, mockUow.Object);
    var command = new CreateTaskCommand("Test", "Desc", 1, DateTime.UtcNow.AddDays(7), userId);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    capturedTask.Should().NotBeNull();
    capturedTask!.Title.Value.Should().Be("Test");
    mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

**Concepts appris** :
- **Mocking** : on isole le handler de la base de données
- **Callback Capture** : on capture l'objet passé au mock pour le vérifier
- **Verify** : on vérifie que SaveChanges a bien été appelé

### Étape 12.6 — Tests EventHandlers (Cross-Module)

```csharp
[Fact]
public async Task Handle_TaskCreatedEvent_ShouldCreateNotification()
{
    // Arrange
    var mockRepo = new Mock<INotificationRepository>();
    var mockUow = new Mock<INotificationUnitOfWork>();
    Notification? captured = null;
    mockRepo.Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Callback<Notification>(n => captured = n);

    var handler = new OnTaskCreated_NotifyUser(mockRepo.Object, mockUow.Object);
    var @event = new TaskCreatedEvent(Guid.NewGuid(), userId, "Ma tâche");

    // Act
    await handler.Handle(@event, CancellationToken.None);

    // Assert
    captured.Should().NotBeNull();
    captured!.Type.Should().Be(NotificationType.TaskCreated);
    captured.UserId.Should().Be(userId);
    mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

### Résultat final : 53 tests ✅

```
TaskFlow.Users.Tests         → 17 tests ✅
TaskFlow.Tasks.Tests         → 22 tests ✅
TaskFlow.Notifications.Tests → 14 tests ✅
─────────────────────────────────────────
Total                        → 53 tests ✅
```

---

## Résumé de tous les concepts clés

| # | Concept | Phase | Où dans le code |
|---|---------|-------|-----------------|
| 1 | Clean Architecture | 1 | Structure des projets |
| 2 | Entity (base class) | 1 | Shared.Kernel/Primitives |
| 3 | Result Pattern | 1 | Shared.Kernel/Results |
| 4 | IDomainEvent | 1 | Shared.Kernel/Primitives |
| 5 | Value Objects | 2 | Domain layers |
| 6 | Rich Domain Model | 2 | Entities avec méthodes métier |
| 7 | Factory Method | 2 | Entity.Create() |
| 8 | Domain Errors | 2 | *Errors.cs |
| 9 | Dependency Inversion (SOLID) | 3 | Interfaces dans Application |
| 10 | CQRS (Command/Query) | 3 | Commands/ et Queries/ |
| 11 | MediatR Pipeline Behavior | 3 | ValidationBehavior |
| 12 | FluentValidation | 3 | *Validator.cs |
| 13 | MediatR Notification (event) | 3 | UserRegisteredNotification |
| 14 | Repository Pattern | 4 | Interface + Implémentation |
| 15 | Unit of Work | 4 | UnitOfWork.cs |
| 16 | EF Core DbContext | 4 | *DbContext.cs |
| 17 | Fluent API Configuration | 4 | *Configuration.cs |
| 18 | Dependency Injection | 4 | DependencyInjection.cs |
| 19 | JWT Authentication | 5 | JwtService + Middleware |
| 20 | ProblemDetails (RFC 7807) | 5 | GlobalExceptionHandler |
| 21 | Serilog (structured logging) | 5 | Program.cs |
| 22 | Domain Events | 6 | TaskCreatedEvent, etc. |
| 23 | Dispatch After SaveChanges | 6 | Tasks.UnitOfWork |
| 24 | Modular Monolith | 6 | DbContexts séparés |
| 25 | Shared Contracts (DTOs) | 7 | Shared.Contracts |
| 26 | Blazor WASM | 8 | TaskFlow.Client |
| 27 | AuthenticationStateProvider | 8 | JwtAuthStateProvider |
| 28 | Pub/Sub (cross-module) | 9 | EventHandlers Notifications |
| 29 | Cross-Module Event Handler | 9 | OnTaskCreated_NotifyUser |
| 30 | DbContext isolé par module | 9 | NotificationsDbContext |
| 31 | Polling côté client | 9 | NotificationService timer |
| 32 | Bootstrap enrichi (theme) | 10 | CSS variables custom |
| 33 | I18n (Internationalisation) | 11 | LanguageService FR/EN |
| 34 | Unit Testing (xUnit) | 12 | *.Tests projects |
| 35 | Mocking (Moq) | 12 | Mock<IRepository> |
| 36 | FluentAssertions | 12 | .Should().BeTrue() |
| 37 | Fixtures (test data) | 12 | *Fixtures.cs |
| 38 | AAA Pattern | 12 | Arrange-Act-Assert |
