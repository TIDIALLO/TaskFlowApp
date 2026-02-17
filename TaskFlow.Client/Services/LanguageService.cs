namespace TaskFlow.Client.Services;

public class LanguageService
{
    private string _lang = "fr";

    public event Action? OnLanguageChanged;

    public string CurrentLanguage => _lang;
    public bool IsFrench => _lang == "fr";

    public void SetLanguage(string lang)
    {
        if (_lang == lang) return;
        _lang = lang;
        OnLanguageChanged?.Invoke();
    }

    public void Toggle() => SetLanguage(_lang == "fr" ? "en" : "fr");

    // ═══ Traductions ═══
    public string Get(string key) => _lang == "fr" ? GetFr(key) : GetEn(key);

    private static string GetFr(string key) => key switch
    {
        // ── Nav ──
        "nav.tasks" => "Mes Tâches",
        "nav.signin" => "Connexion",
        "nav.getstarted" => "Commencer",
        "nav.logout" => "Déconnexion",

        // ── Home ──
        "home.badge" => "Construit avec .NET 8, Blazor WASM & Clean Architecture",
        "home.title1" => "Organisez Votre Travail,",
        "home.title2" => "Décuplez Votre Productivité",
        "home.desc" => "Une application moderne de gestion de tâches avec une architecture modulaire monolithique. Créez, suivez et complétez vos tâches avec des notifications en temps réel.",
        "home.goto_tasks" => "Aller à Mes Tâches",
        "home.notifications" => "Notifications",
        "home.getstarted" => "Commencer — Gratuit",
        "home.signin" => "Se Connecter",
        "home.why" => "Pourquoi TaskFlow ?",
        "home.why_desc" => "Architecture de qualité entreprise, simplifiée.",
        "home.feat1_title" => "Monolithe Modulaire",
        "home.feat1_desc" => "Séparation claire des responsabilités avec des modules indépendants — Users, Tasks et Notifications.",
        "home.feat2_title" => "Pattern CQRS",
        "home.feat2_desc" => "Commandes et Requêtes séparées pour une meilleure scalabilité et un code plus clair.",
        "home.feat3_title" => "Événements Domaine",
        "home.feat3_desc" => "Communication cross-module via MediatR Pub/Sub — totalement découplée et réactive.",
        "home.feat4_title" => "Authentification JWT",
        "home.feat4_desc" => "Authentification stateless sécurisée avec des JSON Web Tokens.",

        // ── Login ──
        "login.title" => "Bon Retour",
        "login.subtitle" => "Connectez-vous pour accéder à TaskFlow",
        "login.email" => "Email",
        "login.password" => "Mot de passe",
        "login.submit" => "Se Connecter",
        "login.no_account" => "Pas de compte ?",
        "login.create_one" => "Créer un compte",
        "login.email_required" => "L'email est requis.",
        "login.email_invalid" => "Format d'email invalide.",
        "login.password_required" => "Le mot de passe est requis.",

        // ── Register ──
        "register.title" => "Créer un Compte",
        "register.subtitle" => "Rejoignez TaskFlow et organisez votre travail",
        "register.firstname" => "Prénom",
        "register.lastname" => "Nom",
        "register.email" => "Email",
        "register.password" => "Mot de passe",
        "register.password_hint" => "Min 8 caractères, 1 majuscule, 1 chiffre.",
        "register.submit" => "Créer le Compte",
        "register.has_account" => "Déjà un compte ?",
        "register.signin" => "Se connecter",
        "register.success" => "Compte créé !",
        "register.signin_now" => "Connectez-vous maintenant",

        // ── Tasks ──
        "tasks.title" => "Mes Tâches",
        "tasks.new" => "Nouvelle Tâche",
        "tasks.create_title" => "Créer une Nouvelle Tâche",
        "tasks.field_title" => "Titre",
        "tasks.field_priority" => "Priorité",
        "tasks.field_duedate" => "Date limite",
        "tasks.field_desc" => "Description",
        "tasks.btn_create" => "Créer",
        "tasks.btn_cancel" => "Annuler",
        "tasks.btn_save" => "Sauvegarder",
        "tasks.btn_start" => "Démarrer",
        "tasks.btn_complete" => "Terminer",
        "tasks.btn_edit" => "Modifier",
        "tasks.btn_delete" => "Supprimer",
        "tasks.delete_confirm" => "Supprimer cette tâche ?",
        "tasks.delete_yes" => "Oui, Supprimer",
        "tasks.delete_no" => "Garder",
        "tasks.edit_title" => "Modifier la Tâche",
        "tasks.loading" => "Chargement des tâches...",
        "tasks.empty" => "Aucune tâche trouvée",
        "tasks.empty_hint" => "Cliquez sur \"+ Nouvelle Tâche\" pour commencer !",
        "tasks.title_required" => "Le titre est requis.",
        "tasks.created" => "Tâche créée !",
        "tasks.updated" => "Tâche mise à jour !",
        "tasks.deleted" => "Tâche supprimée.",
        "tasks.status_changed" => "Statut changé → ",
        "tasks.load_error" => "Échec du chargement des tâches.",
        "tasks.placeholder_title" => "Que faut-il faire ?",
        "tasks.placeholder_desc" => "Détails optionnels...",
        "tasks.completed_at" => "Terminée le",
        "tasks.due" => "Échéance :",

        // Filters
        "filter.all" => "Toutes",
        "filter.todo" => "À faire",
        "filter.inprogress" => "En cours",
        "filter.done" => "Terminées",
        "filter.cancelled" => "Annulées",

        // Priority
        "priority.low" => "Basse",
        "priority.medium" => "Moyenne",
        "priority.high" => "Haute",
        "priority.critical" => "Critique",

        // ── Notifications ──
        "notif.title" => "Notifications",
        "notif.mark_all" => "Tout marquer comme lu",
        "notif.loading" => "Chargement des notifications...",
        "notif.empty" => "Aucune notification",
        "notif.empty_hint" => "Quand vous créez ou terminez des tâches, les notifications apparaîtront ici.",
        "notif.new" => "nouveau",
        "notif.just_now" => "À l'instant",
        "notif.min_ago" => "min",
        "notif.hours_ago" => "h",
        "notif.days_ago" => "j",

        // ── Footer ──
        "footer.text" => "TaskFlow — Construit avec .NET 8, Blazor WASM & Clean Architecture",

        _ => key
    };

    private static string GetEn(string key) => key switch
    {
        // ── Nav ──
        "nav.tasks" => "My Tasks",
        "nav.signin" => "Sign In",
        "nav.getstarted" => "Get Started",
        "nav.logout" => "Logout",

        // ── Home ──
        "home.badge" => "Built with .NET 8, Blazor WASM & Clean Architecture",
        "home.title1" => "Organize Your Work,",
        "home.title2" => "Amplify Your Productivity",
        "home.desc" => "A modern task management app built with modular monolith architecture. Create, track, and complete tasks with real-time notifications.",
        "home.goto_tasks" => "Go to My Tasks",
        "home.notifications" => "Notifications",
        "home.getstarted" => "Get Started — Free",
        "home.signin" => "Sign In",
        "home.why" => "Why TaskFlow?",
        "home.why_desc" => "Enterprise-grade architecture, made simple.",
        "home.feat1_title" => "Modular Monolith",
        "home.feat1_desc" => "Clean separation of concerns with independent modules — Users, Tasks, and Notifications.",
        "home.feat2_title" => "CQRS Pattern",
        "home.feat2_desc" => "Commands and Queries separated for scalability and a cleaner code structure.",
        "home.feat3_title" => "Domain Events",
        "home.feat3_desc" => "Cross-module communication through MediatR Pub/Sub — fully decoupled and reactive.",
        "home.feat4_title" => "JWT Authentication",
        "home.feat4_desc" => "Secure, stateless auth with JSON Web Tokens.",

        // ── Login ──
        "login.title" => "Welcome Back",
        "login.subtitle" => "Sign in to continue to TaskFlow",
        "login.email" => "Email",
        "login.password" => "Password",
        "login.submit" => "Sign In",
        "login.no_account" => "Don't have an account?",
        "login.create_one" => "Create one",
        "login.email_required" => "Email is required.",
        "login.email_invalid" => "Invalid email format.",
        "login.password_required" => "Password is required.",

        // ── Register ──
        "register.title" => "Create Account",
        "register.subtitle" => "Join TaskFlow and organize your work",
        "register.firstname" => "First Name",
        "register.lastname" => "Last Name",
        "register.email" => "Email",
        "register.password" => "Password",
        "register.password_hint" => "Min 8 chars, 1 uppercase, 1 digit.",
        "register.submit" => "Create Account",
        "register.has_account" => "Already have an account?",
        "register.signin" => "Sign in",
        "register.success" => "Account created!",
        "register.signin_now" => "Sign in now",

        // ── Tasks ──
        "tasks.title" => "My Tasks",
        "tasks.new" => "New Task",
        "tasks.create_title" => "Create New Task",
        "tasks.field_title" => "Title",
        "tasks.field_priority" => "Priority",
        "tasks.field_duedate" => "Due Date",
        "tasks.field_desc" => "Description",
        "tasks.btn_create" => "Create",
        "tasks.btn_cancel" => "Cancel",
        "tasks.btn_save" => "Save",
        "tasks.btn_start" => "Start",
        "tasks.btn_complete" => "Complete",
        "tasks.btn_edit" => "Edit",
        "tasks.btn_delete" => "Delete",
        "tasks.delete_confirm" => "Delete this task?",
        "tasks.delete_yes" => "Yes, Delete",
        "tasks.delete_no" => "Keep It",
        "tasks.edit_title" => "Edit Task",
        "tasks.loading" => "Loading tasks...",
        "tasks.empty" => "No tasks found",
        "tasks.empty_hint" => "Click \"+ New Task\" to get started!",
        "tasks.title_required" => "Title is required.",
        "tasks.created" => "Task created!",
        "tasks.updated" => "Task updated!",
        "tasks.deleted" => "Task deleted.",
        "tasks.status_changed" => "Status → ",
        "tasks.load_error" => "Failed to load tasks.",
        "tasks.placeholder_title" => "What needs to be done?",
        "tasks.placeholder_desc" => "Optional details...",
        "tasks.completed_at" => "Completed",
        "tasks.due" => "Due:",

        // Filters
        "filter.all" => "All",
        "filter.todo" => "To Do",
        "filter.inprogress" => "In Progress",
        "filter.done" => "Done",
        "filter.cancelled" => "Cancelled",

        // Priority
        "priority.low" => "Low",
        "priority.medium" => "Medium",
        "priority.high" => "High",
        "priority.critical" => "Critical",

        // ── Notifications ──
        "notif.title" => "Notifications",
        "notif.mark_all" => "Mark All as Read",
        "notif.loading" => "Loading notifications...",
        "notif.empty" => "No notifications yet",
        "notif.empty_hint" => "When you create or complete tasks, notifications will appear here.",
        "notif.new" => "new",
        "notif.just_now" => "Just now",
        "notif.min_ago" => "m ago",
        "notif.hours_ago" => "h ago",
        "notif.days_ago" => "d ago",

        // ── Footer ──
        "footer.text" => "TaskFlow — Built with .NET 8, Blazor WASM & Clean Architecture",

        _ => key
    };
}
