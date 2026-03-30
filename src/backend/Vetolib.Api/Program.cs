using MassTransit;
using Microsoft.AspNetCore.RateLimiting;
using Vetolib.Api;
using Sentry.OpenTelemetry;
using Sentry.Serilog;
using Serilog;
using Serilog.Formatting.Json;
using Vetolib.Agenda;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Api.Audit;
using Vetolib.Api.Dashboard;
using Vetolib.Auth;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing;
using Vetolib.Billing.Infrastructure;
using Vetolib.MedicalRecords;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.AI;
using Vetolib.AI.Infrastructure;
using Vetolib.Messaging;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Notifications;
using Vetolib.Notifications.Infrastructure;
using Vetolib.Stock;
using Vetolib.Stock.Infrastructure;
using Vetolib.Preferences;
using Vetolib.Preferences.Infrastructure;
using Vetolib.Breeding;
using Vetolib.Breeding.Infrastructure;
using Vetolib.ServiceDefaults;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Infrastructure.Email;
using Vetolib.Shared.Kernel;

var builder = WebApplication.CreateBuilder(args);

// Structured logging with Serilog.
// Development : human-readable console output.
// Production  : JSON structured output consumed by the Aspire Dashboard (OTLP)
//               or any log aggregator that reads container stdout (Loki, ELK…).
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Vetolib.Api")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Sentry();

    if (context.HostingEnvironment.IsDevelopment())
    {
        // Human-readable output in the local terminal.
        config.WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}  {Message:lj}{NewLine}{Exception}");
    }
    else
    {
        // JSON to stdout — structured, machine-parseable.
        config.WriteTo.Console(new JsonFormatter());
    }
});

// Aspire ServiceDefaults
builder.AddServiceDefaults();

// Sentry SDK — error tracking + distributed tracing
// DSN is empty by default (SDK disabled). Set Sentry__Dsn env var in production.
builder.WebHost.UseSentry(options =>
{
    var sentryDsn = builder.Configuration["Sentry:Dsn"] ?? "";
    options.Dsn = Uri.IsWellFormedUriString(sentryDsn, UriKind.Absolute) ? sentryDsn : "";
    options.Environment = builder.Environment.EnvironmentName;
    options.TracesSampleRate = builder.Environment.IsProduction() ? 0.3 : 1.0;
    options.SendDefaultPii = false;
    options.Debug = builder.Environment.IsDevelopment();
    options.EnableLogs = true;
    options.UseOpenTelemetry();
});

// ── Fail-fast secret validation ────────────────────────────────────────────
// Secrets must be provided via environment variables or User Secrets.
// The application refuses to start if any required secret is missing or too short.
var jwtSecret = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Jwt:Key is required. Set it via environment variable Jwt__Key " +
        "or, for local development, via: dotnet user-secrets set \"Jwt:Key\" \"<min-32-chars>\"");

if (jwtSecret.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Key must be at least 32 characters long.");

// Connection string is injected by .NET Aspire at runtime; in non-Aspire environments
// (e.g. docker compose) it must be set via ConnectionStrings__vetolibdb env var.
// We validate it here so the error is obvious at startup, not buried in EF Core logs.
_ = builder.Configuration.GetConnectionString("vetolibdb")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:vetolibdb is required. Set it via environment variable " +
        "ConnectionStrings__vetolibdb or let .NET Aspire inject it automatically.");
// ───────────────────────────────────────────────────────────────────────────

// Database — Multi-tenant DbContexts use AddDbContext (NOT pooled) because they depend
// on scoped IClinicContext. Aspire's AddNpgsqlDbContext uses pooling by default, which
// resolves dependencies from the root provider and fails with scoped services.
// EnrichNpgsqlDbContext adds Aspire telemetry/retries without changing the registration.
var connectionString = builder.Configuration.GetConnectionString("vetolibdb")!;

builder.Services.AddAuthDbContext(connectionString);
builder.EnrichNpgsqlDbContext<AuthDbContext>(settings => settings.DisableHealthChecks = true);

builder.Services.AddAgendaDbContext(connectionString);
builder.EnrichNpgsqlDbContext<AgendaDbContext>(settings => settings.DisableHealthChecks = true);

builder.Services.AddMedicalRecordsDbContext(connectionString);
builder.EnrichNpgsqlDbContext<MedicalRecordsDbContext>(settings => settings.DisableHealthChecks = true);

builder.Services.AddBillingDbContext(connectionString);
builder.EnrichNpgsqlDbContext<BillingDbContext>(settings => settings.DisableHealthChecks = true);

// Audit context — no IClinicContext dependency, pooling is fine
builder.AddNpgsqlDbContext<AuditDbContext>("vetolibdb", settings => settings.DisableHealthChecks = true);

builder.Services.AddMessagingDbContext(connectionString);
builder.EnrichNpgsqlDbContext<MessagingDbContext>(settings => settings.DisableHealthChecks = true);

// Notifications context — no IClinicContext dependency, pooling is fine
builder.AddNpgsqlDbContext<NotificationsDbContext>("vetolibdb", settings => settings.DisableHealthChecks = true);

// Multi-tenancy
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IClinicContext, ClinicContext>();

// Audit trail — singleton interceptor wired to each module DbContext via IDbContextOptionsConfiguration
builder.Services.AddSharedInfrastructure();
builder.Services.AddAuditInterceptor<AuthDbContext>();
builder.Services.AddAuditInterceptor<AgendaDbContext>();
builder.Services.AddAuditInterceptor<MedicalRecordsDbContext>();
builder.Services.AddAuditInterceptor<BillingDbContext>();

// Email service (SmtpEmailSender pointing to MailHog in dev)
builder.Services.AddEmailSender(builder.Configuration);

// MassTransit — integration events + EF Core Outbox
builder.Services.AddMassTransit(x =>
{
    // Auto-register all consumers from the Notifications module assembly
    x.AddConsumers(typeof(NotificationsModuleServiceRegistrar).Assembly);

    // EF Core Outbox — guarantees at-least-once delivery for integration events.
    // Each module DbContext that publishes events gets its own outbox tables.
    x.AddEntityFrameworkOutbox<AuthDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.AddEntityFrameworkOutbox<AgendaDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.AddEntityFrameworkOutbox<BillingDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.AddEntityFrameworkOutbox<NotificationsDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.AddEntityFrameworkOutbox<MessagingDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    // Use RabbitMQ transport (connection string injected by Aspire via "rabbitmq" resource)
    var rabbitMqConnectionString = builder.Configuration.GetConnectionString("rabbitmq");

    if (rabbitMqConnectionString is not null)
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(rabbitMqConnectionString);
            cfg.ConfigureEndpoints(context);
        });
    }
    else
    {
        // Fallback: in-memory transport for local dev without Aspire / tests
        x.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    }
});

// Auth module
builder.Services.AddAuthModule(builder.Configuration);

// Agenda module
builder.Services.AddAgendaModule(builder.Configuration);

// MedicalRecords module
builder.Services.AddMedicalRecordsModule(builder.Configuration);

// Billing module
builder.Services.AddBillingModule(builder.Configuration);

// Notifications module
builder.Services.AddNotificationsModule();

// AI module (triage + no-show prediction)
builder.Services.AddAIModule(builder.Configuration);
builder.Services.AddAIDbContext(connectionString);
builder.EnrichNpgsqlDbContext<AIDbContext>(settings => settings.DisableHealthChecks = true);

// Messaging module
builder.Services.AddMessagingModule(builder.Configuration);

// Stock module
builder.Services.AddStockModule(builder.Configuration);
builder.Services.AddStockDbContext(connectionString);
builder.EnrichNpgsqlDbContext<StockDbContext>(settings => settings.DisableHealthChecks = true);

// Preferences module
builder.Services.AddPreferencesModule(builder.Configuration);
builder.Services.AddPreferencesDbContext(connectionString);
builder.EnrichNpgsqlDbContext<PreferencesDbContext>(settings => settings.DisableHealthChecks = true);

// Breeding module
builder.Services.AddBreedingModule(builder.Configuration);
builder.Services.AddBreedingDbContext(connectionString);
builder.EnrichNpgsqlDbContext<BreedingDbContext>(settings => settings.DisableHealthChecks = true);

// CORS — allow configured origins (defaults to localhost for dev)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000", "https://localhost:3000"];
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Rate limiting
// "auth"   — 10 req/min per IP (login, refresh, change-password)
// "signup" — 3 req/h per IP   (clinic self-registration)
// "api"    — 100 req/min per IP (all other authenticated endpoints)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddSlidingWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 6;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("signup", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromHours(1);
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

// JSON: accept string enum values in request bodies (e.g., "MedicalQuestion" instead of 2).
// Also serializes enum responses as strings for consistency.
// All step definitions that read enum-containing DTOs must use JsonStringEnumConverter too.
builder.Services.ConfigureHttpJsonOptions(opts =>
    opts.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Global exception handler — returns generic 500 JSON without stack traces
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { title = "An unexpected error occurred.", status = 500 });
    });
});

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapDefaultEndpoints();
app.MapAuthEndpoints();
app.MapAgendaEndpoints();
app.MapMedicalRecordsEndpoints();
app.MapBillingEndpoints();
app.MapAuditApiEndpoints();
app.MapDashboardApiEndpoints();
app.MapAIEndpoints();
app.MapMessagingEndpoints();
app.MapStockEndpoints();
app.MapPreferencesEndpoints();
app.MapBreedingEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Database initialization — migrations + seed data
await DbInitializer.MigrateAllAsync(app.Services);
await DbInitializer.SeedAsync(app.Services);
await DbInitializer.SeedDrugCatalogAsync(app.Services);

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
