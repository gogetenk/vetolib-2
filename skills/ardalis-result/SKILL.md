# Skill: Ardalis.Result — Pattern Result PARTOUT

## Règle absolue

**Zéro exception pour le control flow business.**
Chaque méthode qui peut échouer pour une raison métier retourne `Result<T>` ou `Result`.
Cette règle s'applique dans toutes les couches : Domain, Application, Api.

## NuGet packages

```xml
<PackageReference Include="Ardalis.Result" Version="10.*" />
<PackageReference Include="Ardalis.Result.AspNetCore" Version="10.*" />
<PackageReference Include="Ardalis.Result.FluentValidation" Version="10.*" />
```

## Les statuts disponibles

| Méthode | Usage | HTTP |
|---|---|---|
| `Result.Success()` | Opération réussie sans valeur | 200 |
| `Result<T>.Success(value)` | Opération réussie avec valeur | 200 |
| `Result<T>.NotFound()` | Entité inexistante | 404 |
| `Result<T>.Invalid(errors)` | Validation échouée | 400 |
| `Result.Unauthorized()` | Non authentifié | 401 |
| `Result.Forbidden()` | Non autorisé | 403 |
| `Result.Error(messages)` | Erreur métier non-validation | 422 |

## Dans le Domain — entités et méthodes de factory

```csharp
// Méthode factory statique — retourne Result<T> pour la création
internal class Appointment : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public AppointmentStatus Status { get; private set; } = AppointmentStatus.Pending;

    // Factory statique avec validation
    public static Result<Appointment> Create(
        Guid clinicId,
        Guid vetId,
        Guid patientId,
        DateTime scheduledAt,
        int durationMinutes)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));
        if (scheduledAt <= DateTime.UtcNow)
            errors.Add(new ValidationError(nameof(scheduledAt), "Cannot schedule in the past"));
        if (durationMinutes is < 15 or > 240)
            errors.Add(new ValidationError(nameof(durationMinutes), "Duration must be between 15 and 240 minutes"));

        if (errors.Any())
            return Result<Appointment>.Invalid(errors);

        return Result<Appointment>.Success(new Appointment
        {
            ClinicId = clinicId,
            VetId = vetId,
            PatientId = patientId,
            ScheduledAt = scheduledAt,
            DurationMinutes = durationMinutes,
            Status = AppointmentStatus.Pending
        });
    }

    // Méthodes de transition d'état
    public Result Confirm()
    {
        if (Status != AppointmentStatus.Pending)
            return Result.Error($"Cannot confirm appointment in status '{Status}'.");
        Status = AppointmentStatus.Confirmed;
        AddDomainEvent(new AppointmentConfirmedEvent(Id, ClinicId));
        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (Status == AppointmentStatus.Done)
            return Result.Error("Cannot cancel a completed appointment.");
        Status = AppointmentStatus.Cancelled;
        CancelReason = reason;
        return Result.Success();
    }
}
```

## Dans l'Application — Handlers MediatR

```csharp
internal class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IAppointmentRepository _repo;
    private readonly IVetRepository _vetRepo;

    public async Task<Result<AppointmentDto>> Handle(
        CreateAppointmentCommand cmd,
        CancellationToken ct)
    {
        // Vérifications NotFound
        var vet = await _vetRepo.GetByIdAsync(cmd.VetId, ct);
        if (vet is null)
            return Result<AppointmentDto>.NotFound($"Vet '{cmd.VetId}' not found.");

        // Appel de la factory Domain — résultat propagé
        var appointmentResult = Appointment.Create(
            cmd.ClinicId, cmd.VetId, cmd.PatientId, cmd.ScheduledAt, cmd.DurationMinutes);

        if (!appointmentResult.IsSuccess)
            return appointmentResult.Map(_ => (AppointmentDto)null!);

        // Vérification de conflit (règle métier)
        var hasConflict = await _repo.HasConflictAsync(cmd.VetId, cmd.ScheduledAt, cmd.DurationMinutes, ct);
        if (hasConflict)
            return Result<AppointmentDto>.Conflict();  // ou .Error("Slot already taken")

        await _repo.AddAsync(appointmentResult.Value, ct);
        await _repo.SaveChangesAsync(ct);

        return Result<AppointmentDto>.Success(appointmentResult.Value.ToDto());
    }
}

// Query handler
internal class GetAppointmentByIdHandler : IRequestHandler<GetAppointmentByIdQuery, Result<AppointmentDto>>
{
    public async Task<Result<AppointmentDto>> Handle(GetAppointmentByIdQuery query, CancellationToken ct)
    {
        var appointment = await _repo.GetByIdAsync(query.AppointmentId, ct);
        if (appointment is null)
            return Result<AppointmentDto>.NotFound();
        return Result<AppointmentDto>.Success(appointment.ToDto());
    }
}
```

## Validation avec FluentValidation + Ardalis.Result

```csharp
// Le ValidationBehavior intercepte AVANT le handler
internal class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .SelectMany(v => v.Validate(context).Errors)
            .Where(f => f is not null)
            .ToList();

        if (!failures.Any()) return await next();

        // Convertir FluentValidation errors en Ardalis.Result ValidationError
        var errors = failures.Select(f => new ValidationError(f.PropertyName, f.ErrorMessage));
        return (TResponse)(object)Result.Invalid(errors);
    }
}

// Validator classique FluentValidation
internal class CreateAppointmentValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.VetId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ScheduledAt).GreaterThan(DateTime.UtcNow).WithMessage("Must be in the future");
        RuleFor(x => x.DurationMinutes).InclusiveBetween(15, 240);
    }
}
```

## Dans l'Api — Minimal APIs

```csharp
// TOUJOURS .ToMinimalApiResult() — jamais de switch/if sur le résultat dans les endpoints
internal static class AppointmentEndpoints
{
    internal static IEndpointRouteBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments")
            .RequireAuthorization()
            .WithTags("Appointments");

        group.MapPost("/", Create).WithName("CreateAppointment");
        group.MapGet("/{id:guid}", GetById).WithName("GetAppointmentById");
        group.MapPut("/{id:guid}/confirm", Confirm).WithName("ConfirmAppointment");
        group.MapDelete("/{id:guid}", Cancel).WithName("CancelAppointment");

        return app;
    }

    private static async Task<IResult> Create(CreateAppointmentCommand cmd, ISender sender)
        => (await sender.Send(cmd)).ToMinimalApiResult();

    private static async Task<IResult> GetById(Guid id, ISender sender)
        => (await sender.Send(new GetAppointmentByIdQuery(id))).ToMinimalApiResult();

    private static async Task<IResult> Confirm(Guid id, ISender sender)
        => (await sender.Send(new ConfirmAppointmentCommand(id))).ToMinimalApiResult();

    private static async Task<IResult> Cancel(Guid id, CancelAppointmentRequest req, ISender sender)
        => (await sender.Send(new CancelAppointmentCommand(id, req.Reason))).ToMinimalApiResult();
}
```

## Railway Oriented Programming — Map et Bind

```csharp
// Map : transformer la valeur en cas de succès
var dtoResult = appointmentResult.Map(a => a.ToDto());

// Bind : chaîner des opérations qui retournent Result
var finalResult = await appointmentResult
    .Bind(a => a.Confirm())          // Result<Appointment> → Result
    .Map(() => "Confirmed");          // Result → Result<string>
```

## Anti-patterns

```
❌ throw new NotFoundException("...")    → Result.NotFound()
❌ throw new ValidationException(...)   → Result.Invalid(errors)
❌ return null                          → Result.NotFound()
❌ try { ... } catch (Exception e) { return BadRequest(e.Message); }
   → Le ValidationBehavior gère ça
❌ if (result.IsSuccess) return Ok(result.Value); else return NotFound();
   → result.ToMinimalApiResult() fait tout
```

## Configuration dans Program.cs

```csharp
// Enregistrer la convention pour Swagger/OpenAPI
builder.Services.AddControllers(opts =>
    opts.AddResultConvention(resultStatusMap =>
        resultStatusMap.AddDefaultMap()));
```
