# 📋 TaskFlow — Documentation d'Architecture

## Table des matières

1. [Vue d'ensemble](#vue-densemble)
2. [Architecture globale](#architecture-globale)
3. [Structure des dossiers](#structure-des-dossiers)
4. [Modules](#modules)
5. [Flux d'une requête](#flux-dune-requête)
6. [Patterns & Principes](#patterns--principes)
7. [Domain Events & Communication inter-modules](#domain-events--communication-inter-modules)
8. [Base de données](#base-de-données)
9. [Authentification JWT](#authentification-jwt)
10. [Frontend Blazor WASM](#frontend-blazor-wasm)
11. [Internationalisation (I18n)](#internationalisation-i18n)
12. [Tests unitaires](#tests-unitaires)
13. [Endpoints API](#endpoints-api)
14. [Comment lancer le projet](#comment-lancer-le-projet)

---

## Vue d'ensemble

**TaskFlow** est une application de gestion de tâches construite avec une architecture **Modular Monolith** et **Clean Architecture / DDD**.

| Composant | Technologie |
|-----------|-------------|
| **Backend API** | ASP.NET Core 8 Web API |
| **Frontend** | Blazor WebAssembly (standalone) |
| **Base de données** | SQL Server (LocalDB) |
| **ORM** | Entity Framework Core 8 |
| **CQRS/Mediator** | MediatR 12 |
| **Validation** | FluentValidation |
| **Auth** | JWT Bearer Tokens |
| **Hashing** | BCrypt.Net |
| **Logging** | Serilog (Console + Fichier) |
| **Tests** | xUnit + FluentAssertions + Moq |
| **CSS** | Bootstrap 5 + Custom Theme |
| **I18n** | LanguageService custom (FR/EN) |

---

## Architecture globale

Le projet suit une **Clean Architecture** organisée en **3 modules** (Modular Monolith) :

```
┌─────────────────────────────────────────────────────────┐
│                    TaskFlow.Client                       │  ← Blazor WASM (navigateur)
│         (Pages, Services, Auth, LanguageService)         │
└──────────────────────────┬──────────────────────────────┘
                           │ HTTP (JSON + JWT)
┌──────────────────────────▼──────────────────────────────┐
│                    TaskFlow.Api                          │  ← ASP.NET Core Web API
│            (Controllers, Middleware)                      │
└──────┬──────────────────┬──────────────────┬────────────┘
       │                  │                  │
┌──────▼────────┐  ┌──────▼────────┐  ┌──────▼──────────────┐
│ Module Users  │  │ Module Tasks  │  │ Module Notifications │
│ ┌───────────┐ │  │ ┌───────────┐ │  │ ┌────────────────┐  │
│ │Application│ │  │ │Application│ │  │ │  Application   │  │  ← Use Cases (CQRS)
│ │  (CQRS)   │ │  │ │  (CQRS)   │ │  │ │  + EventHdlrs │  │
│ ├───────────┤ │  │ ├───────────┤ │  │ ├────────────────┤  │
│ │  Domain   │ │  │ │  Domain   │ │  │ │    Domain      │  │  ← Entités, Events
│ ├───────────┤ │  │ ├───────────┤ │  │ ├────────────────┤  │
│ │Infra      │ │  │ │Infra      │ │  │ │    Infra       │  │  ← EF Core, Repos
│ └───────────┘ │  │ └───────────┘ │  │ └────────────────┘  │
└───────────────┘  └───────────────┘  └──────────────────────┘
       │                  │                  │
       │   Pub/Sub (MediatR INotification)   │
       │   TaskCreatedEvent ───────────────► │
       │   TaskCompletedEvent ─────────────► │
       │   UserRegisteredNotification ─────► │
       │                  │                  │
┌──────▼──────────────────▼──────────────────▼────────────┐
│           Shared.Kernel + Shared.Contracts              │  ← Types partagés
└──────────────────────────┬──────────────────────────────┘
                           │
                   ┌───────▼───────┐
                   │  SQL Server   │
                   │  (LocalDB)    │
                   └───────────────┘
```

### Principe de dépendances (Clean Architecture)

```
Domain ← Application ← Infrastructure ← API
  (0 dépendance)  (MediatR)    (EF Core)     (Controllers)
```

- **Domain** ne dépend de RIEN (pur C#)
- **Application** dépend du Domain (entités, value objects)
- **Infrastructure** dépend de Application (interfaces) — implémente les contrats
- **API** dépend de Infrastructure (pour le DI) et Application (pour les commands/queries)

---

## Structure des dossiers

```
TaskFlow/
├── TaskFlow.Api/                    # Point d'entrée HTTP
│   ├── Controllers/                 # Routes REST
│   │   ├── ApiController.cs         # Base : Result → IActionResult
│   │   ├── UsersController.cs       # /api/users/*
│   │   ├── TasksController.cs       # /api/tasks/*
│   │   └── NotificationsController.cs  # /api/notifications/*
│   ├── Middleware/
│   │   └── GlobalExceptionHandler.cs # Catch-all pour les erreurs 500
│   └── Program.cs                   # Configuration & pipeline
│
├── TaskFlow.Client/                 # Blazor WebAssembly (navigateur)
│   ├── Auth/
│   │   ├── JwtAuthStateProvider.cs  # Décode JWT → ClaimsPrincipal
│   │   └── RedirectToLogin.razor    # Redirige si non autorisé
│   ├── Layout/
│   │   ├── MainLayout.razor         # Structure HTML globale
│   │   └── NavMenu.razor            # Barre de navigation + lang toggle
│   ├── Pages/
│   │   ├── Home.razor               # / — Landing page professionnelle
│   │   ├── Login.razor              # /login
│   │   ├── Register.razor           # /register
│   │   ├── Tasks.razor              # /tasks — Dashboard des tâches
│   │   └── Notifications.razor      # /notifications — Centre de notifs
│   ├── Services/
│   │   ├── AuthService.cs           # Login/Register/Logout API calls
│   │   ├── TaskService.cs           # CRUD tâches API calls
│   │   ├── NotificationService.cs   # Notifications API calls
│   │   └── LanguageService.cs       # I18n FR/EN singleton
│   └── wwwroot/                     # Fichiers statiques (CSS, fonts)
│       ├── css/app.css              # Custom theme (CSS variables)
│       └── index.html               # Host HTML + Bootstrap + Icons
│
├── TaskFlow.Shared.Kernel/          # Types fondamentaux partagés
│   ├── Primitives/
│   │   ├── Entity.cs                # Classe de base (Id, Equals, DomainEvents)
│   │   └── IDomainEvent.cs          # Marker interface → MediatR.INotification
│   └── Results/
│       ├── Result.cs                # Result pattern (Success/Failure)
│       └── Error.cs                 # Error + ErrorType enum
│
├── TaskFlow.Shared.Contracts/       # DTOs partagés Client ↔ API
│   ├── Auth/
│   │   ├── LoginRequest.cs
│   │   ├── RegisterRequest.cs
│   │   └── AuthResponse.cs
│   ├── Tasks/
│   │   ├── CreateTaskRequest.cs
│   │   ├── UpdateTaskRequest.cs
│   │   ├── ChangeStatusRequest.cs
│   │   └── TaskItemResponse.cs
│   └── Notifications/
│       ├── NotificationResponse.cs
│       └── UnreadCountResponse.cs
│
├── TaskFlow.Users.Domain/           # Domaine métier Users
│   ├── Entities/User.cs
│   ├── ValueObjects/                # Email, Password, FullName
│   ├── Errors/UserErrors.cs
│   └── Specifications/
│
├── TaskFlow.Users.Application/      # Use Cases Users
│   ├── Commands/
│   │   ├── Register/                # RegisterUserCommand + Handler + Validator
│   │   └── Login/                   # LoginCommand + Handler + Validator
│   ├── Queries/
│   │   ├── GetUserById/
│   │   └── GetAllUsers/
│   ├── Interfaces/                  # IUserRepository, IJwtService, IPasswordHasher
│   ├── Behaviors/ValidationBehavior.cs  # Pipeline MediatR : validation auto
│   └── Notifications/               # UserRegisteredNotification (event)
│
├── TaskFlow.Users.Infrastructure/   # Implémentations concrètes Users
│   ├── Data/
│   │   ├── UsersDbContext.cs
│   │   └── UnitOfWork.cs
│   ├── Repositories/UserRepository.cs
│   ├── Services/
│   │   ├── JwtService.cs            # Génération de tokens JWT
│   │   └── PasswordHasher.cs        # BCrypt hash/verify
│   ├── Configurations/              # EF Core Fluent API
│   └── Migrations/
│
├── TaskFlow.Tasks.Domain/           # Domaine métier Tasks
│   ├── Entities/TaskItem.cs         # Rich Domain Model + Domain Events
│   ├── Enums/                       # Priority, TaskItemStatus
│   ├── ValueObjects/                # TaskTitle, TaskDescription
│   ├── Events/                      # TaskCreatedEvent, TaskCompletedEvent, TaskStatusChangedEvent
│   └── Errors/TaskItemErrors.cs
│
├── TaskFlow.Tasks.Application/      # Use Cases Tasks
│   ├── Commands/
│   │   ├── CreateTask/              # Command + Handler + Validator
│   │   ├── UpdateTask/
│   │   ├── ChangeTaskStatus/
│   │   └── DeleteTask/
│   ├── Queries/
│   │   ├── GetTaskById/
│   │   └── GetUserTasks/
│   ├── Interfaces/                  # ITaskItemRepository, IUnitOfWork
│   └── Mappings/TaskItemMappings.cs # Entity → DTO
│
├── TaskFlow.Tasks.Infrastructure/   # Implémentations concrètes Tasks
│   ├── Data/
│   │   ├── TasksDbContext.cs
│   │   └── UnitOfWork.cs            # Dispatch Domain Events after SaveChanges
│   ├── Repositories/TaskItemRepository.cs
│   ├── Configurations/
│   └── Migrations/
│
├── TaskFlow.Notifications.Domain/   # Domaine métier Notifications
│   ├── Entities/Notification.cs     # Entity avec factory Create()
│   └── Enums/NotificationType.cs    # Welcome, TaskCreated, TaskCompleted, etc.
│
├── TaskFlow.Notifications.Application/  # Use Cases Notifications
│   ├── Commands/
│   │   ├── MarkAsRead/              # Marquer une notif lue
│   │   └── MarkAllAsRead/           # Marquer toutes lues
│   ├── Queries/
│   │   ├── GetUserNotifications/    # Lister les notifs d'un user
│   │   └── GetUnreadCount/          # Compter les non-lues
│   ├── EventHandlers/               # ⚡ Cross-module event handlers
│   │   ├── OnUserRegistered_CreateWelcomeNotification.cs
│   │   ├── OnTaskCreated_NotifyUser.cs
│   │   └── OnTaskCompleted_CongratulateUser.cs
│   ├── Interfaces/                  # INotificationRepository, INotificationUnitOfWork
│   └── Mappings/NotificationMappings.cs
│
├── TaskFlow.Notifications.Infrastructure/  # Implémentations Notifications
│   ├── Data/
│   │   ├── NotificationsDbContext.cs
│   │   └── NotificationUnitOfWork.cs
│   ├── Repositories/NotificationRepository.cs
│   └── Configurations/
│
├── TaskFlow.Users.Tests/            # Tests unitaires Users
│   ├── Domain/                      # Value Objects (Email, Password, FullName)
│   └── Application/                 # Command Handlers (Register, Login)
│
├── TaskFlow.Tasks.Tests/            # Tests unitaires Tasks
│   ├── Fixtures/TaskFixtures.cs     # Données de test réutilisables
│   ├── Domain/
│   │   ├── ValueObjects/            # TaskTitle, TaskDescription
│   │   └── Entities/                # TaskItem lifecycle tests
│   └── Application/
│       └── Commands/                # CreateTask, ChangeTaskStatus handlers
│
├── TaskFlow.Notifications.Tests/    # Tests unitaires Notifications
│   ├── Fixtures/NotificationFixtures.cs
│   ├── Domain/Entities/             # Notification entity tests
│   └── Application/
│       ├── Commands/                # MarkAsRead, MarkAllAsRead handlers
│       └── EventHandlers/           # Cross-module event handler tests
│
├── docs/                            # Documentation
│   ├── ARCHITECTURE.md              # Ce fichier
│   └── DEVELOPMENT-STEPS.md         # Étapes pas à pas
│
└── TaskFlow.sln                     # Solution Visual Studio
```

---

## Modules

### Module Users

**Responsabilité** : inscription, connexion, gestion des utilisateurs. Publie `UserRegisteredNotification`.

| Couche | Contenu |
|--------|---------|
| **Domain** | `User` (Entity), `Email`, `Password`, `FullName` (Value Objects), `UserErrors` |
| **Application** | `RegisterUserCommand`, `LoginCommand`, `GetUserByIdQuery`, `GetAllUsersQuery`, `ValidationBehavior`, `UserRegisteredNotification` |
| **Infrastructure** | `UsersDbContext`, `UserRepository`, `JwtService`, `PasswordHasher`, `UnitOfWork` |

### Module Tasks

**Responsabilité** : CRUD des tâches, gestion du cycle de vie (Todo → InProgress → Done/Cancelled). Publie des **Domain Events**.

| Couche | Contenu |
|--------|---------|
| **Domain** | `TaskItem` (Entity), `TaskTitle`, `TaskDescription` (Value Objects), `Priority`, `TaskItemStatus` (Enums), `TaskCreatedEvent`, `TaskCompletedEvent`, `TaskStatusChangedEvent` |
| **Application** | `CreateTaskCommand`, `UpdateTaskCommand`, `ChangeTaskStatusCommand`, `DeleteTaskCommand`, `GetTaskByIdQuery`, `GetUserTasksQuery` |
| **Infrastructure** | `TasksDbContext`, `TaskItemRepository`, `UnitOfWork` (with event dispatch) |

### Module Notifications *(nouveau)*

**Responsabilité** : gestion des notifications utilisateur. **Écoute** les événements des autres modules via MediatR Pub/Sub.

| Couche | Contenu |
|--------|---------|
| **Domain** | `Notification` (Entity), `NotificationType` (Enum) |
| **Application** | `MarkAsReadCommand`, `MarkAllAsReadCommand`, `GetUserNotificationsQuery`, `GetUnreadCountQuery`, 3 EventHandlers cross-module |
| **Infrastructure** | `NotificationsDbContext`, `NotificationRepository`, `NotificationUnitOfWork` |

### Graphe de dépendances entre projets

```
TaskFlow.Shared.Kernel              ← (aucune dépendance)
TaskFlow.Shared.Contracts           ← (aucune dépendance)

TaskFlow.Users.Domain               ← Shared.Kernel
TaskFlow.Users.Application          ← Users.Domain, Shared.Contracts
TaskFlow.Users.Infrastructure       ← Users.Application, Users.Domain, Shared.Kernel

TaskFlow.Tasks.Domain               ← Shared.Kernel
TaskFlow.Tasks.Application          ← Tasks.Domain, Shared.Contracts
TaskFlow.Tasks.Infrastructure       ← Tasks.Application, Tasks.Domain, Shared.Kernel

TaskFlow.Notifications.Domain       ← Shared.Kernel
TaskFlow.Notifications.Application  ← Notifications.Domain, Shared.Contracts, Shared.Kernel,
                                       Users.Application (events), Tasks.Domain (events)
TaskFlow.Notifications.Infrastructure ← Notifications.Application, Notifications.Domain, Shared.Kernel

TaskFlow.Api                        ← Users.Infra, Tasks.Infra, Notifications.Infra, Shared.Contracts
TaskFlow.Client                     ← Shared.Contracts

TaskFlow.Users.Tests                ← Users.Application, Users.Domain, Shared.Kernel
TaskFlow.Tasks.Tests                ← Tasks.Application, Tasks.Domain, Shared.Kernel
TaskFlow.Notifications.Tests        ← Notifications.Application, Notifications.Domain, Shared.Kernel,
                                       Users.Application (events), Tasks.Domain (events)
```

---

## Flux d'une requête

### Exemple : Créer une tâche (avec Domain Events)

```
1. [Blazor Page]  → L'utilisateur remplit le formulaire et clique "Create"
        │
2. [TaskService]  → Appelle POST /api/tasks avec le JWT en header
        │
3. [TasksController.Create()]  → Extrait le UserId du JWT
        │                         → Crée un CreateTaskCommand
        │                         → Envoie via MediatR
        │
4. [ValidationBehavior]  → FluentValidation vérifie les données
        │                    (titre non vide, priorité valide, etc.)
        │
5. [CreateTaskCommandHandler]  → Crée les Value Objects (TaskTitle, TaskDescription)
        │                        → Appelle TaskItem.Create() (factory method)
        │                        → TaskItem ajoute un TaskCreatedEvent dans DomainEvents
        │                        → Ajoute au Repository
        │
6. [UnitOfWork.SaveChangesAsync()]
        │   → Persiste dans la DB
        │   → Collecte les DomainEvents de toutes les entités
        │   → Publie chaque event via MediatR.Publish()
        │   → ClearDomainEvents()
        │
7. [OnTaskCreated_NotifyUser]  ← EventHandler dans le module Notifications
        │   → Crée une Notification "Nouvelle tâche créée"
        │   → Persiste via INotificationRepository + SaveChanges
        │
8. [Réponse]  → Handler retourne Result<TaskItemResponse>
              → Controller retourne 201 Created + JSON
              → Blazor affiche la nouvelle tâche
              → Le badge de notifications se met à jour (polling)
```

---

## Patterns & Principes

### 1. Result Pattern (au lieu des Exceptions)

```csharp
// ❌ Mauvais — exceptions pour le flux métier
public User GetById(Guid id)
{
    var user = _repo.Find(id);
    if (user == null) throw new NotFoundException(); // coûteux, pas explicite
    return user;
}

// ✅ Bon — Result pattern
public Result<User> GetById(Guid id)
{
    var user = _repo.Find(id);
    if (user == null) return Result<User>.Failure(UserErrors.NotFound);
    return Result<User>.Success(user);
}
```

**Pourquoi** : les exceptions sont lentes et cachent les cas d'erreur. `Result<T>` rend les erreurs **explicites** dans le type de retour.

### 2. CQRS avec MediatR

- **Command** = intention de MODIFIER (Create, Update, Delete) → retourne `Result<T>`
- **Query** = intention de LIRE (Get, List) → retourne `Result<T>`
- **Handler** = traite UNE command/query → Single Responsibility Principle

```csharp
// Command (le "message")
public record CreateTaskCommand(string Title, ...) : IRequest<Result<TaskItemResponse>>;

// Handler (le "traitement")
public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskItemResponse>>
{
    public async Task<Result<TaskItemResponse>> Handle(CreateTaskCommand request, CancellationToken ct)
    {
        // orchestration : validation → création → persistance → mapping
    }
}
```

### 3. Rich Domain Model

L'entité contient les **règles métier**, pas les services :

```csharp
// L'entité protège ses propres invariants
public Result Start()
{
    if (Status != TaskItemStatus.Todo)
        return Result.Failure(TaskItemErrors.CannotStart);
    Status = TaskItemStatus.InProgress;
    AddDomainEvent(new TaskStatusChangedEvent(Id, UserId, "InProgress"));
    return Result.Success();
}
```

### 4. Value Objects

Encapsulent la validation et le comportement :

```csharp
public sealed class Email
{
    public string Value { get; }
    private Email(string value) => Value = value;

    public static Result<Email> Create(string value)
    {
        if (!IsValidEmail(value))
            return Result<Email>.Failure(UserErrors.InvalidEmail);
        return Result<Email>.Success(new Email(value));
    }
}
```

### 5. Repository + Unit of Work

- **Repository** : encapsule l'accès aux données (Add, GetById, etc.)
- **Unit of Work** : gère la transaction (`SaveChangesAsync`) + **dispatch les Domain Events**
- Les deux sont des **interfaces** dans Application, **implémentées** dans Infrastructure

### 6. Factory Method

Les entités ont un constructeur privé + méthode statique `Create()` :

```csharp
public static Result<TaskItem> Create(TaskTitle title, ..., Guid userId)
{
    if (dueDate < DateTime.UtcNow) return Result.Failure(...);
    var task = new TaskItem(Guid.NewGuid(), title, ...);
    task.AddDomainEvent(new TaskCreatedEvent(task.Id, userId, title.Value));
    return Result.Success(task);
}
```

### 7. Pub/Sub (MediatR Notifications)

Communication **découplée** entre modules :

```csharp
// Module Tasks publie un événement (via UnitOfWork)
public record TaskCreatedEvent(Guid TaskId, Guid UserId, string Title) : IDomainEvent;

// Module Notifications écoute (sans connaître Tasks.Infrastructure)
public class OnTaskCreated_NotifyUser : INotificationHandler<TaskCreatedEvent>
{
    public async Task Handle(TaskCreatedEvent notification, CancellationToken ct)
    {
        var notif = Notification.Create(notification.UserId, NotificationType.TaskCreated, ...);
        await _repository.AddAsync(notif);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

### 8. Dispatch After SaveChanges

Les events sont publiés **après** la persistance pour garantir la cohérence :

```csharp
// UnitOfWork.SaveChangesAsync()
var result = await _context.SaveChangesAsync(ct);  // 1. Persister
var events = /* collect DomainEvents from tracked entities */;
foreach (var domainEvent in events)
    await _mediator.Publish(domainEvent, ct);       // 2. Publier
entity.ClearDomainEvents();                          // 3. Nettoyer
```

---

## Domain Events & Communication inter-modules

### Schéma de communication

```
┌─────────────┐                          ┌──────────────────────┐
│ Module Users │                          │ Module Notifications │
│             │  UserRegisteredNotif.     │                      │
│ Register ──►├─────────────────────────►│ WelcomeNotification  │
│ Handler     │  (MediatR Publish)        │ EventHandler         │
└─────────────┘                          └──────────────────────┘

┌─────────────┐                          ┌──────────────────────┐
│ Module Tasks│                          │ Module Notifications │
│             │  TaskCreatedEvent         │                      │
│ Create ────►├─────────────────────────►│ TaskCreatedHandler   │
│ (Entity)    │                           │                      │
│             │  TaskCompletedEvent       │                      │
│ Complete ──►├─────────────────────────►│ CongratulateHandler  │
│ (Entity)    │  (via UnitOfWork)         │                      │
└─────────────┘                          └──────────────────────┘
```

### Types d'événements

| Événement | Source | Listener | Action |
|-----------|--------|----------|--------|
| `UserRegisteredNotification` | Users.Application | Notifications.Application | Crée notif "Bienvenue" |
| `TaskCreatedEvent` | Tasks.Domain (Entity) | Notifications.Application | Crée notif "Tâche créée" |
| `TaskCompletedEvent` | Tasks.Domain (Entity) | Notifications.Application | Crée notif "Félicitations" |
| `TaskStatusChangedEvent` | Tasks.Domain (Entity) | *(extensible)* | Non utilisé encore |

### Deux types de publication

1. **MediatR Notification (Application Event)** : publié manuellement dans le Handler (`_mediator.Publish(new UserRegisteredNotification(...))`)
2. **Domain Event** : publié automatiquement par le `UnitOfWork` après `SaveChanges` (collecte les events des entities)

---

## Base de données

**Type** : SQL Server LocalDB
**Connection String** : `Server=(localdb)\\mssqllocaldb;Database=TaskFlowDb`

### Tables

| Table | Module | Colonnes clés |
|-------|--------|---------------|
| `Users` | Users | Id, Email, Password (hash), FirstName, LastName, IsActive, CreatedAt |
| `Tasks` | Tasks | Id, Title, Description, Priority, Status, DueDate, UserId (FK logique), CreatedAt, CompletedAt |
| `Notifications` | Notifications | Id, UserId, Type, Title, Message, IsRead, CreatedAt |

### Trois DbContexts séparés (isolation modulaire)

```csharp
UsersDbContext         → DbSet<User> Users
TasksDbContext         → DbSet<TaskItem> Tasks
NotificationsDbContext → DbSet<Notification> Notifications
```

Chaque module a son propre DbContext. Ils partagent la même base de données mais ne "voient" que leurs propres tables. C'est le principe **Shared Database, Isolated Contexts** du Modular Monolith.

---

## Authentification JWT

### Flux d'authentification

```
1. POST /api/users/login { email, password }
   → Vérifie email + BCrypt.Verify(password, hash)
   → Génère un JWT contenant : UserId, Email, Name
   → Retourne { token: "eyJhbG..." }

2. Client stocke le JWT dans localStorage

3. Chaque requête suivante inclut :
   Authorization: Bearer eyJhbG...

4. Le middleware JWT décode le token et injecte les Claims
   → HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) → UserId
```

### Structure du JWT

```json
{
  "header": { "alg": "HS256", "typ": "JWT" },
  "payload": {
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "guid-user-id",
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "user@email.com",
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "John Doe",
    "exp": 1708099200
  }
}
```

### Shared Identity via Token

Les modules ne partagent **pas de base utilisateur** directement. Le `UserId` voyage dans le JWT :

```
Controller → extrait UserId du JWT → passe en paramètre au Command/Query
→ chaque module filtre ses données par UserId
```

---

## Frontend Blazor WASM

### Architecture Client

```
Blazor WASM s'exécute DANS le navigateur (WebAssembly).
Il n'y a PAS de serveur côté client — c'est du pur client-side.

┌─────────── Navigateur ───────────┐
│  ┌─────────────────────────┐     │
│  │     Pages (.razor)      │     │
│  │  (Home, Login, Register,│     │
│  │   Tasks, Notifications) │     │
│  └──────────┬──────────────┘     │
│             │ @inject             │
│  ┌──────────▼──────────────┐     │
│  │     Services            │     │
│  │  (AuthService,          │     │
│  │   TaskService,          │     │
│  │   NotificationService,  │     │
│  │   LanguageService)      │     │
│  └──────────┬──────────────┘     │
│             │ HttpClient          │
│  ┌──────────▼──────────────┐     │
│  │  JwtAuthStateProvider   │     │
│  │  + localStorage (JWT)   │     │
│  └──────────┬──────────────┘     │
└─────────────┼────────────────────┘
              │ HTTPS + Bearer Token
       ┌──────▼──────┐
       │  API Backend │
       └─────────────┘
```

### Pages et routes

| Route | Page | Auth requise | Description |
|-------|------|--------------|-------------|
| `/` | Home.razor | Non | Landing page professionnelle |
| `/login` | Login.razor | Non | Connexion |
| `/register` | Register.razor | Non | Inscription |
| `/tasks` | Tasks.razor | **Oui** | Dashboard des tâches |
| `/notifications` | Notifications.razor | **Oui** | Centre de notifications |

### UI/UX Design

- **CSS Framework** : Bootstrap 5 enrichi avec des CSS variables custom
- **Palette** : Indigo (#4F46E5) / Slate (#1E293B) / Blanc
- **Police** : Inter (Google Fonts)
- **Icônes** : Bootstrap Icons
- **Animations** : Fadeins subtils, hover effects, gradients
- **Responsive** : Mobile-first via Bootstrap grid

---

## Internationalisation (I18n)

### Architecture

Le service `LanguageService` (singleton) gère les traductions FR/EN :

```csharp
public class LanguageService
{
    public string CurrentLanguage { get; private set; } = "FR";
    public event Action? OnLanguageChanged;

    public void SetLanguage(string lang) { ... OnLanguageChanged?.Invoke(); }
    public string T(string key) => _translations[CurrentLanguage][key];
}
```

### Fonctionnement

1. Le `LanguageService` contient un dictionnaire `FR` et `EN` avec toutes les clés
2. Chaque composant Razor injecte `LanguageService` et appelle `Lang.T("key")`
3. Le toggle de langue dans `NavMenu.razor` appelle `SetLanguage()`
4. L'événement `OnLanguageChanged` notifie tous les composants → `StateHasChanged()`
5. L'interface se re-rend dans la nouvelle langue **sans rechargement**

---

## Tests unitaires

### Architecture des tests

```
┌──────────────────────────────────────────────────────┐
│                     53 Tests                         │
├──────────────┬──────────────────┬────────────────────┤
│ Users.Tests  │  Tasks.Tests     │ Notifications.Tests│
│  (existant)  │   (nouveau)      │   (nouveau)        │
├──────────────┼──────────────────┼────────────────────┤
│ Domain:      │ Domain:          │ Domain:            │
│  Email       │  TaskTitle       │  Notification      │
│  Password    │  TaskDescription │                    │
│  FullName    │  TaskItem        │ Application:       │
│  User        │                  │  MarkAsRead        │
│              │ Application:     │  MarkAllAsRead     │
│ Application: │  CreateTask      │                    │
│  Register    │  ChangeStatus    │ EventHandlers:     │
│  Login       │                  │  OnUserRegistered  │
│              │ Fixtures:        │  OnTaskCreated     │
│              │  TaskFixtures    │  OnTaskCompleted   │
│              │                  │                    │
│              │                  │ Fixtures:          │
│              │                  │  NotifFixtures     │
└──────────────┴──────────────────┴────────────────────┘
```

### Patterns de test

| Pattern | Description | Exemple |
|---------|-------------|---------|
| **AAA** | Arrange-Act-Assert | Structure standard de chaque test |
| **`[Fact]`** | Test unique | `MarkAsRead_Should_SetIsReadTrue()` |
| **`[Theory]`** | Tests paramétrés | `Create_WithInvalidTitle_ShouldFail(string title)` |
| **Mocking** | Isolation via Moq | `Mock<ITaskItemRepository>` |
| **Fixtures** | Données réutilisables | `TaskFixtures.CreateValidTask()` |
| **Callback Capture** | Vérifier les args | `Setup(...).Callback<T>(t => captured = t)` |

### Commandes

```bash
# Lancer tous les tests
dotnet test

# Lancer les tests d'un module
dotnet test TaskFlow.Tasks.Tests

# Lancer avec détails
dotnet test --verbosity normal
```

---

## Endpoints API

### Users (`/api/users`)

| Méthode | Route | Auth | Description |
|---------|-------|------|-------------|
| `POST` | `/api/users/register` | ❌ | Inscription (publie UserRegisteredNotification) |
| `POST` | `/api/users/login` | ❌ | Connexion (retourne JWT) |
| `GET` | `/api/users/{id}` | ✅ | Détail d'un utilisateur |
| `GET` | `/api/users` | ✅ | Liste des utilisateurs |

### Tasks (`/api/tasks`)

| Méthode | Route | Auth | Description |
|---------|-------|------|-------------|
| `POST` | `/api/tasks` | ✅ | Créer une tâche (déclenche TaskCreatedEvent) |
| `GET` | `/api/tasks` | ✅ | Mes tâches |
| `GET` | `/api/tasks/{id}` | ✅ | Détail d'une tâche |
| `PUT` | `/api/tasks/{id}` | ✅ | Modifier une tâche |
| `PATCH` | `/api/tasks/{id}/status` | ✅ | Changer le statut (déclenche events) |
| `DELETE` | `/api/tasks/{id}` | ✅ | Supprimer une tâche |

### Notifications (`/api/notifications`)

| Méthode | Route | Auth | Description |
|---------|-------|------|-------------|
| `GET` | `/api/notifications` | ✅ | Mes notifications |
| `GET` | `/api/notifications/unread-count` | ✅ | Nombre de non-lues |
| `PATCH` | `/api/notifications/{id}/read` | ✅ | Marquer une notif lue |
| `PATCH` | `/api/notifications/read-all` | ✅ | Marquer toutes lues |

### Cycle de vie d'une tâche (avec events)

```
     ┌──────────┐
     │   Todo   │ ← état initial (+ TaskCreatedEvent)
     └────┬─────┘
          │ Start() → TaskStatusChangedEvent
     ┌────▼──────────┐
     │  InProgress   │
     └────┬──────┬───┘
          │      │
Complete()│      │ Cancel()
  + Event │      │ + Event
          │      │
     ┌────▼───┐ ┌▼──────────┐
     │  Done  │ │ Cancelled  │
     └────────┘ └────────────┘
```

---

## Comment lancer le projet

### Prérequis

- .NET 8 SDK
- SQL Server LocalDB (inclus avec Visual Studio)

### Commandes

```bash
# 1. Restaurer les packages
dotnet restore

# 2. Appliquer les migrations (créer la base)
dotnet ef database update --project TaskFlow.Users.Infrastructure --startup-project TaskFlow.Api
dotnet ef database update --project TaskFlow.Tasks.Infrastructure --startup-project TaskFlow.Api

# 3. Lancer l'API (terminal 1)
cd TaskFlow.Api
dotnet run

# 4. Lancer le frontend (terminal 2)
cd TaskFlow.Client
dotnet run

# 5. Ouvrir dans le navigateur
# API Swagger : https://localhost:7239/swagger
# Frontend    : http://localhost:5082
```

### Lancer les tests

```bash
# Tous les tests (53 tests, 3 modules)
dotnet test

# Tests avec détails
dotnet test --verbosity normal

# Un module spécifique
dotnet test TaskFlow.Tasks.Tests
dotnet test TaskFlow.Notifications.Tests
dotnet test TaskFlow.Users.Tests
```

---

## Résumé des concepts & patterns

| Concept | Où dans le code | Pourquoi |
|---------|----------------|----------|
| Clean Architecture | Structure des projets | Séparation des responsabilités |
| DDD (Domain-Driven Design) | Domain Layer | Modèle riche, règles métier dans les entités |
| Modular Monolith | 3 modules (Users, Tasks, Notifications) | Isolation, préparation microservices |
| CQRS | Application Layer | Séparer lecture/écriture |
| MediatR | Commands/Queries/Handlers | Découplage Controller ↔ Logique |
| Result Pattern | Shared.Kernel | Pas d'exceptions pour le flux métier |
| Value Objects | Domain | Validation à la création, toujours valide |
| Repository Pattern | Application (interface) + Infrastructure (impl) | Abstraction de l'accès aux données |
| Unit of Work | Infrastructure | Transaction unique + dispatch events |
| Domain Events | Entity.AddDomainEvent() | Communication intra-module découplée |
| Pub/Sub (MediatR Notifications) | EventHandlers cross-module | Communication inter-module découplée |
| Dispatch After SaveChanges | UnitOfWork | Cohérence : events après persistance |
| IDomainEvent (Marker Interface) | Shared.Kernel | Contrat commun pour events cross-module |
| Factory Method | Entity.Create() | Validation + construction atomique |
| JWT Authentication | Infrastructure + API middleware | Authentification stateless |
| Shared Identity via Token | JWT claims | UserId partagé sans couplage |
| DbContext isolé par module | UsersDbContext, TasksDbContext, NotificationsDbContext | Isolation des données |
| Polling côté client | Blazor Timer (NotificationService) | Rafraîchissement des notifications |
| I18n (Internationalisation) | LanguageService (singleton) | Support FR/EN |
| ValidationBehavior | MediatR Pipeline | Validation automatique cross-cutting |
| FluentValidation | Validators | Règles de validation déclaratives |
| ProblemDetails (RFC 7807) | GlobalExceptionHandler | Réponses d'erreur standardisées |
| Serilog | Program.cs | Logging structuré |
