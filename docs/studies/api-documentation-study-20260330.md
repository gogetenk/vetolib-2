# API Documentation Study — 2026-03-30

## 1. Current OpenAPI Setup

### Configuration

- **Package**: `Microsoft.AspNetCore.OpenApi` v10.* (the built-in ASP.NET Core 10 OpenAPI support)
- **Registration**: `builder.Services.AddOpenApi()` in `Program.cs` (line 288)
- **Mapping**: `app.MapOpenApi()` gated behind `app.Environment.IsDevelopment()` (line 323-326)
- **No UI**: There is no Swagger UI, Scalar, or any other interactive documentation viewer configured. The OpenAPI JSON is served at `/openapi/v1.json` but there is no human-readable UI to browse it.
- **No Scalar or Swashbuckle**: Only the native Microsoft OpenAPI package is referenced.

### What works today

- OpenAPI JSON is generated automatically from endpoint signatures in dev mode
- `JsonStringEnumConverter` is registered globally, so enums appear as strings in the spec

---

## 2. Endpoint Inventory

### Summary by Module

| Module | Tag(s) | Endpoint Count | Endpoint File(s) |
|---|---|---|---|
| **Auth** | Auth, Clinics, Users, ClinicGroups, Onboarding | 16 | AuthEndpoints, ClinicEndpoints, UserEndpoints, ClinicGroupEndpoints, OnboardingEndpoints |
| **Agenda** | Appointments, ConsultationTypes | 12 (8 primary + 4 legacy aliases) | AppointmentEndpoints, ConsultationTypeEndpoints |
| **MedicalRecords** | Patients, MedicalRecords, Owners, Weights, DrugCatalog, Prescriptions | 15 | PatientEndpoints, MedicalRecordEndpoints, OwnerEndpoints, WeightEndpoints, DrugCatalogEndpoints |
| **Billing** | Invoices, EReporting | 10 | InvoiceEndpoints, EReportingEndpoints |
| **AI** | AI, AI - Health Alerts | 12 | AIEndpoints, HealthAlertEndpoints |
| **Messaging** | Messaging, WhatsApp, OwnerPortal | ~30 | MessagingEndpoints, WhatsAppEndpoints, PortalEndpoints, SseEndpoints |
| **Notifications** | Reminders | 3 | ReminderEndpoints |
| **Stock** | Stock | 5 | StockEndpoints |
| **Breeding** | Breeding, Breeding - Pregnancies, Litters, HeatCycles | 12 | BreedingEndpoints, PregnancyEndpoints, LitterEndpoints, HeatCycleEndpoints |
| **Preferences** | (none) | 0 | ModuleServiceRegistrar (scaffold only) |
| **Dashboard** | Dashboard | 4 | DashboardEndpoints |
| **Audit** | Audit | 1 | AuditEndpoints |

**Total: ~120 endpoints across 12 modules**

---

## 3. Current Documentation Quality Per Endpoint

### What IS present on most endpoints

| Metadata | Coverage | Notes |
|---|---|---|
| `.WithTags()` | ~95% of groups | Almost all endpoint groups set a tag. Good. |
| `.WithName()` | ~95% of endpoints | Operation IDs are set, enabling client code generation. Good. |
| `.RequireAuthorization()` / `.AllowAnonymous()` | 100% | Every endpoint has auth metadata. Good for security. |
| `.RequireRateLimiting()` | Selective | Applied to auth, signup, and general API groups. |

### What is MISSING

| Metadata | Coverage | Impact |
|---|---|---|
| `.WithSummary()` | **1 out of ~120 endpoints** (only AuditEndpoints) | API consumers have no human-readable description of what each endpoint does |
| `.WithDescription()` | **0 endpoints** | No detailed descriptions anywhere |
| `.Produces<T>()` | **1 endpoint** (RegisterClinic only) | OpenAPI spec shows `200 OK` for everything, no typed error responses. Consumers cannot see the response shape for errors. |
| `.ProducesValidationProblem()` | **0 endpoints** | Validation errors (400/422) are not documented in the spec |
| `.ProducesProblem()` | **0 endpoints** | Error responses (404, 409, 500) are not documented |
| Request/response examples | **0 endpoints** | No `WithOpenApi()` customization for examples |
| Authentication scheme in spec | **Not configured** | The OpenAPI spec does not declare a Bearer token security scheme, so tools like Scalar/SwaggerUI cannot show the "Authorize" button |
| Deprecation markers | **0** | Legacy `/api/appointments` aliases have no deprecation flag |
| Pagination documentation | **0** | List endpoints accept `page`/`pageSize` query params but this is not explicitly documented in the spec beyond what ASP.NET infers |

---

## 4. Specific Issues Found

### 4.1 No API documentation UI

The raw JSON at `/openapi/v1.json` is not usable by developers without a viewer. There is no Swagger UI or Scalar configured.

### 4.2 OpenAPI only in Development

`app.MapOpenApi()` is wrapped in `if (app.Environment.IsDevelopment())`. This means staging/production environments have no OpenAPI spec at all. For a SaaS product targeting external integrators (clinic management software), the API spec should be available in at least staging.

### 4.3 No security scheme declaration

The spec does not declare JWT Bearer as a security scheme. Even though endpoints have `RequireAuthorization()`, the OpenAPI spec does not communicate HOW to authenticate. API consumers and code generators need this.

### 4.4 Ardalis.Result response mapping is opaque

All endpoints use `result.ToMinimalApiResult()` which maps:
- `Result.Success` -> 200 OK
- `Result.NotFound` -> 404
- `Result.Invalid` -> 422 or 400
- `Result.Unauthorized` -> 401
- `Result.Forbidden` -> 403

But none of these are declared via `.Produces()`, so the OpenAPI spec only shows the happy path.

### 4.5 Legacy endpoints not marked deprecated

`AppointmentEndpoints.cs` registers a `legacyGroup` at `/api/appointments` (without `/v1/`) that mirrors several endpoints. These are not marked `.WithMetadata(new ObsoleteAttribute())` or equivalent, so they appear as first-class endpoints in the spec.

### 4.6 Messaging module has undocumented SSE endpoint

The SSE endpoint at `/api/v1/messaging/sse` produces `text/event-stream` but this is not declared in the OpenAPI spec. SSE endpoints need special treatment in API docs.

---

## 5. Recommendations

### Priority 1 — API Documentation UI (low effort, high impact)

Add **Scalar** as the OpenAPI viewer. It is the modern replacement for Swagger UI, supports .NET 10 natively, and provides a better developer experience.

```csharp
// Program.cs — add after MapOpenApi()
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Add Scalar UI at /scalar/v1
}
```

NuGet: `Scalar.AspNetCore`

### Priority 2 — Security scheme declaration (low effort, critical for API consumers)

Configure the OpenAPI security scheme so that the Bearer token appears in the spec:

```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Title = "Vetolib API";
        document.Info.Version = "v1";
        document.Info.Description = "Veterinary clinic management platform API";
        // Add JWT Bearer security scheme
        return Task.CompletedTask;
    });
});
```

Or use the OpenAPI document transformer to inject the `securitySchemes` and global `security` requirement.

### Priority 3 — `.WithSummary()` and `.WithDescription()` on all endpoints (medium effort, high impact)

Add human-readable summaries to every endpoint. This is the single biggest improvement for API consumers. Example pattern:

```csharp
group.MapPost("/", CreateAppointment)
    .WithName("CreateAppointment")
    .WithSummary("Book a new appointment")
    .WithDescription("Creates a new appointment for a patient with a veterinarian. Checks for time slot conflicts automatically.");
```

**Suggested approach**: Do it module-by-module, one PR per module. Start with Auth (most used by integrators) and MedicalRecords (most complex).

### Priority 4 — `.Produces()` on all endpoints (medium effort, high impact)

Declare success and error response types. A reusable extension could reduce boilerplate:

```csharp
// Extension method for the common Ardalis.Result pattern
public static RouteHandlerBuilder ProducesStandardResult<T>(this RouteHandlerBuilder builder)
    => builder
        .Produces<T>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
```

### Priority 5 — Mark legacy endpoints as deprecated (low effort)

```csharp
legacyGroup.MapPost("/", CreateAppointment)
    .WithMetadata(new ObsoleteAttribute("Use /api/v1/appointments instead"));
```

Or use `.WithOpenApi(op => { op.Deprecated = true; return op; })`.

### Priority 6 — Request/response examples (high effort, nice-to-have)

Use `.WithOpenApi()` to add examples using the ASP.NET Core 10 OpenAPI transformers. This is more work but valuable for complex endpoints like `ImportPatients` or `CreateInvoice`.

### Priority 7 — Consider enabling OpenAPI in staging

Change the gate from `IsDevelopment()` to include staging:

```csharp
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
```

This allows QA and integration partners to browse the API spec on staging without exposing it in production.

---

## 6. Effort Estimation

| Task | Effort | Files Touched |
|---|---|---|
| Add Scalar UI | 30 min | Program.cs, Vetolib.Api.csproj |
| Security scheme declaration | 1h | Program.cs |
| `.WithSummary()` on all ~120 endpoints | 4-6h | ~24 endpoint files |
| `.Produces()` on all endpoints + helper extension | 4-6h | ~24 endpoint files + 1 new extension |
| Deprecate legacy endpoints | 30 min | AppointmentEndpoints.cs |
| Request/response examples (selected endpoints) | 4-8h | Selected endpoint files |
| Enable OpenAPI in staging | 15 min | Program.cs |

**Total estimated effort: 2-3 days of focused work**

---

## 7. Files Referenced

- `src/backend/Vetolib.Api/Program.cs` — OpenAPI setup, middleware pipeline
- `src/backend/Vetolib.Api/Vetolib.Api.csproj` — NuGet packages
- `src/backend/Vetolib.Api/Audit/AuditEndpoints.cs` — only endpoint with `.WithSummary()`
- `src/backend/Modules/Auth/Vetolib.Auth/Api/ClinicEndpoints.cs` — only endpoint with `.Produces()`
- `src/backend/Modules/Auth/Vetolib.Auth/Api/AuthEndpoints.cs` — auth flow endpoints
- `src/backend/Modules/Agenda/Vetolib.Agenda/Api/AppointmentEndpoints.cs` — legacy endpoint aliases
- `src/backend/Modules/MedicalRecords/Vetolib.MedicalRecords/Api/PatientEndpoints.cs` — typical endpoint pattern
- `src/backend/Modules/Messaging/Vetolib.Messaging/Api/SseEndpoints.cs` — SSE endpoint
- All 24 `*Endpoints.cs` files across modules
