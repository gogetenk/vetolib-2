using Vetolib.Agenda;
using Vetolib.Agenda.Infrastructure;
using Vetolib.Auth;
using Vetolib.Auth.Infrastructure;
using Vetolib.Billing;
using Vetolib.Billing.Infrastructure;
using Vetolib.MedicalRecords;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.ServiceDefaults;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;

var builder = WebApplication.CreateBuilder(args);

// Aspire ServiceDefaults
builder.AddServiceDefaults();

// Database — Aspire Npgsql integration
builder.AddNpgsqlDbContext<AuthDbContext>("vetolibdb");
builder.AddNpgsqlDbContext<AgendaDbContext>("vetolibdb");
builder.AddNpgsqlDbContext<MedicalRecordsDbContext>("vetolibdb");
builder.AddNpgsqlDbContext<BillingDbContext>("vetolibdb");

// Multi-tenancy
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IClinicContext, ClinicContext>();

// Auth module
builder.Services.AddAuthModule(builder.Configuration);

// Agenda module
builder.Services.AddAgendaModule(builder.Configuration);

// MedicalRecords module
builder.Services.AddMedicalRecordsModule(builder.Configuration);

// Billing module
builder.Services.AddBillingModule(builder.Configuration);

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

app.MapOpenApi();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
