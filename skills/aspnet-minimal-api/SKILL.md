# Skill: ASP.NET Core 10 — Minimal APIs

## Principes

- Pas de Controllers. Uniquement des Minimal API endpoints.
- Chaque module expose une méthode d'extension `MapXxxEndpoints(this IEndpointRouteBuilder)`.
- Toujours terminer par `.ToMinimalApiResult()` — jamais de mappage manuel.
- Les endpoints ne contiennent AUCUNE logique métier. Ils délèguent à MediatR.

## Pattern de fichier d'endpoints

```csharp
// Agenda/Api/AppointmentEndpoints.cs
internal static class AppointmentEndpoints
{
    internal static IEndpointRouteBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments")
            .RequireAuthorization()
            .WithTags("Appointments")
            .WithOpenApi();

        group.MapGet("/", GetAll)
            .WithName("GetAppointments")
            .Produces<IReadOnlyList<AppointmentDto>>();

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetAppointmentById")
            .Produces<AppointmentDto>()
            .ProducesProblem(404);

        group.MapPost("/", Create)
            .WithName("CreateAppointment")
            .Produces<AppointmentDto>(201)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}/confirm", Confirm)
            .WithName("ConfirmAppointment")
            .Produces(200)
            .ProducesProblem(404);

        group.MapDelete("/{id:guid}", Cancel)
            .WithName("CancelAppointment")
            .Produces(204);

        return app;
    }

    // Chaque handler = 1 ligne + ToMinimalApiResult()
    private static async Task<IResult> GetAll(
        [AsParameters] GetAppointmentsQuery query,
        ISender sender)
        => (await sender.Send(query)).ToMinimalApiResult();

    private static async Task<IResult> GetById(Guid id, ISender sender)
        => (await sender.Send(new GetAppointmentByIdQuery(id))).ToMinimalApiResult();

    private static async Task<IResult> Create(
        CreateAppointmentRequest req,
        IClinicContext clinicContext,
        ISender sender)
    {
        // ClinicId injecté depuis le contexte, jamais depuis le body
        var cmd = new CreateAppointmentCommand(
            ClinicId: clinicContext.ClinicId,
            VetId: req.VetId,
            PatientId: req.PatientId,
            ScheduledAt: req.ScheduledAt,
            DurationMinutes: req.DurationMinutes,
            Notes: req.Notes);
        return (await sender.Send(cmd)).ToMinimalApiResult();
    }

    private static async Task<IResult> Confirm(Guid id, ISender sender)
        => (await sender.Send(new ConfirmAppointmentCommand(id))).ToMinimalApiResult();

    private static async Task<IResult> Cancel(
        Guid id,
        CancelAppointmentRequest req,
        ISender sender)
        => (await sender.Send(new CancelAppointmentCommand(id, req.Reason))).ToMinimalApiResult();
}
```

## Extension publique dans ModuleServiceRegistrar

```csharp
// AgendaModuleServiceRegistrar.cs
public static IEndpointRouteBuilder MapAgendaEndpoints(this IEndpointRouteBuilder app)
    => AppointmentEndpoints.MapAppointmentEndpoints(app);
```

## Program.cs — registration de tous les modules

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();  // Aspire

// Modules
builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddAgendaModule(builder.Configuration);
builder.Services.AddMedicalRecordsModule(builder.Configuration);
builder.Services.AddBillingModule(builder.Configuration);

// Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts => { /* config */ });
builder.Services.AddAuthorization();

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Endpoints de chaque module
app.MapDefaultEndpoints();  // /health, /alive (Aspire)
app.MapAuthEndpoints();
app.MapAgendaEndpoints();
app.MapMedicalRecordsEndpoints();
app.MapBillingEndpoints();

app.MapOpenApi();

app.Run();
```

## Codes HTTP — conventions

| Opération | Status code |
|---|---|
| GET (liste) | 200 avec array |
| GET (item trouvé) | 200 avec item |
| GET (non trouvé) | 404 Problem Details |
| POST (créé) | 201 avec item + Location header |
| PUT/PATCH (mis à jour) | 200 avec item mis à jour |
| DELETE | 204 No Content |
| Validation échouée | 400 Problem Details avec errors |
| Non authentifié | 401 |
| Non autorisé | 403 |
| Erreur métier | 422 Problem Details |

## Injection des paramètres — binding

```csharp
// Query string → [AsParameters]
group.MapGet("/", GetAll);
private static Task<IResult> GetAll([AsParameters] GetAppointmentsQuery query, ISender sender)
    => ...;

// Route param → Guid id (automatique)
group.MapGet("/{id:guid}", GetById);
private static Task<IResult> GetById(Guid id, ISender sender) => ...;

// Body → record request (automatique avec application/json)
group.MapPost("/", Create);
private static Task<IResult> Create(CreateAppointmentRequest req, ISender sender) => ...;

// Services injectés → par type (DI automatique)
private static Task<IResult> Create(
    CreateAppointmentRequest req,   // body
    IClinicContext clinicCtx,       // DI
    ISender sender)                 // DI
```

## Problem Details — format d'erreur uniforme

```json
// 400 Bad Request (Result.Invalid)
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "ScheduledAt": ["Must be in the future"],
        "VetId": ["VetId is required"]
    }
}

// 404 Not Found (Result.NotFound)
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    "title": "Resource not found",
    "status": 404
}
```

## Anti-patterns

```
❌ Controllers (zero toleration)
❌ Logique métier dans les endpoints
❌ try/catch dans les endpoints pour le business flow
❌ return Ok(value) / return NotFound() — utiliser .ToMinimalApiResult()
❌ Exposer ClinicId dans les requêtes client (c'est du JWT, toujours serveur)
❌ Mapper manuellement Result vers IResult
```

## Pagination — pattern standardisé

```csharp
// Requête paginée
internal record GetAppointmentsQuery(
    int Page = 1,
    int PageSize = 20,
    DateTime? From = null,
    DateTime? To = null
) : IRequest<Result<PagedResult<AppointmentDto>>>;

// Réponse paginée (dans Shared.Kernel)
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```
