using MassTransit;
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
using Vetolib.Stock;
using Vetolib.Stock.Infrastructure;
using Vetolib.Preferences;
using Vetolib.Preferences.Infrastructure;
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
    options.Dsn = builder.Configuration["Sentry:Dsn"] ?? "";
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

// Database — Aspire Npgsql integration
// Disable connection pooling: our DbContexts depend on scoped IClinicContext (multi-tenancy),
// which is incompatible with DbContext pooling (resolves from root provider).
builder.AddNpgsqlDbContext<AuthDbContext>("vetolibdb", settings => settings.DisableHealthChecks = true);
builder.AddNpgsqlDbContext<AgendaDbContext>("vetolibdb", settings => settings.DisableHealthChecks = true);
builder.AddNpgsqlDbContext<MedicalRecordsDbContext>("vetolibdb", settings => settings.DisableHealthChecks = true);
builder.AddNpgsqlDbContext<BillingDbContext>("vetolibdb", settings => settings.DisableHealthChecks = true);
// Audit context — dedicated context for the shared.audit_log table
builder.AddNpgsqlDbContext<AuditDbContext>("vetolibdb", settings => settings.DisableHealthChecks = true);
builder.AddNpgsqlDbContext<MessagingDbContext>("vetolibdb", settings => settings.DisableHealthChecks = true);

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
builder.AddNpgsqlDbContext<AIDbContext>("vetolibdb", settings => settings.DisableHealthChecks = true);

// Messaging module
builder.Services.AddMessagingModule(builder.Configuration);

// Stock module
builder.Services.AddStockModule(builder.Configuration);
builder.AddNpgsqlDbContext<StockDbContext>("vetolibdb", settings => settings.DisableHealthChecks = true);

// Preferences module
builder.Services.AddPreferencesModule(builder.Configuration);
builder.AddNpgsqlDbContext<PreferencesDbContext>("vetolibdb", settings => settings.DisableHealthChecks = true);

// JSON: accept string enum values in request bodies (e.g., "MedicalQuestion" instead of 2).
// Also serializes enum responses as strings for consistency.
// All step definitions that read enum-containing DTOs must use JsonStringEnumConverter too.
builder.Services.ConfigureHttpJsonOptions(opts =>
    opts.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

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

app.MapOpenApi();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
