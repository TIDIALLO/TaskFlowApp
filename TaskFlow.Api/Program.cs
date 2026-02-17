using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using TaskFlow.Api.Middleware;
using TaskFlow.Tasks.Application;
using TaskFlow.Tasks.Infrastructure;
using TaskFlow.Users.Application;
using TaskFlow.Users.Infrastructure;
using TaskFlow.Notifications.Application;
using TaskFlow.Notifications.Infrastructure;

// ═══════════════════════════════════════════════════════════
// SERILOG — Configuré en premier car il doit capturer les erreurs de démarrage
// ═══════════════════════════════════════════════════════════
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/taskflow-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting TaskFlow API...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // ═══════════════════════════════════════════════════════════
    // SERVICES — Chaque module s'enregistre proprement via ses extension methods
    // ═══════════════════════════════════════════════════════════
    builder.Services
        .AddUsersApplication()                                    // MediatR, Validators, Behaviors
        .AddUsersInfrastructure(builder.Configuration)            // EF Core, Repos, JWT, BCrypt
        .AddTasksApplication()                                    // MediatR, Validators pour Tasks
        .AddTasksInfrastructure(builder.Configuration)            // EF Core, Repos pour Tasks
        .AddNotificationsApplication()                            // MediatR handlers cross-module
        .AddNotificationsModule(builder.Configuration);           // EF Core, Repos pour Notifications

    builder.Services.AddControllers();

    // Global Exception Handler (IExceptionHandler de .NET 8)
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails(); // Active le format ProblemDetails pour les erreurs

    // ═══════════════════════════════════════════════════════════
    // JWT AUTHENTICATION
    // ═══════════════════════════════════════════════════════════
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
            };
        });

    builder.Services.AddAuthorization();

    // ═══════════════════════════════════════════════════════════
    // CORS — Permet au frontend Blazor WASM d'appeler l'API
    // ═══════════════════════════════════════════════════════════
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowBlazor", policy =>
        {
            policy
                .WithOrigins("http://localhost:5082") // URL du frontend Blazor WASM
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    // ═══════════════════════════════════════════════════════════
    // SWAGGER
    // ═══════════════════════════════════════════════════════════
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "TaskFlow API",
            Version = "v1",
            Description = "Task management API"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // ═══════════════════════════════════════════════════════════
    // MIDDLEWARE PIPELINE — L'ordre compte !
    // ═══════════════════════════════════════════════════════════
    var app = builder.Build();

    app.UseExceptionHandler();      // 1. Attrape les exceptions en premier
    app.UseSerilogRequestLogging(); // 2. Log chaque requête HTTP

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowBlazor");     // 3. CORS avant auth
    app.UseAuthentication();         // 4. Vérifie le token JWT
    app.UseAuthorization();          // 5. Vérifie les autorisations
    app.MapControllers();

    Log.Information("TaskFlow API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
