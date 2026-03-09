# Skill: CQRS avec MediatR

## Principe

- **Commands** = opérations d'écriture. Retournent `Result` ou `Result<T>`.
- **Queries** = opérations de lecture. Retournent `Result<T>`. Jamais d'effet de bord.
- **Behaviors** = pipeline cross-cutting : validation, logging, performance.
- **Notifications** = Domain Events. Fire-and-forget vers d'autres handlers.

## Structure de fichiers

```
Application/
├── Commands/
│   └── CreateAppointment/
│       ├── CreateAppointmentCommand.cs       ← record (params d'entrée)
│       ├── CreateAppointmentHandler.cs       ← logique use case
│       └── CreateAppointmentValidator.cs     ← FluentValidation rules
├── Queries/
│   └── GetAppointments/
│       ├── GetAppointmentsQuery.cs
│       └── GetAppointmentsHandler.cs
└── Behaviors/
    ├── ValidationBehavior.cs
    └── LoggingBehavior.cs
```

## Command — pattern exact

```csharp
// Command = record immutable avec tous les paramètres
internal record CreateAppointmentCommand(
    Guid ClinicId,       // Injecté depuis IClinicContext (pas envoyé par le client)
    Guid VetId,
    Guid PatientId,
    DateTime ScheduledAt,
    int DurationMinutes,
    string? Notes
) : IRequest<Result<AppointmentDto>>;

// Handler
internal class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IAppointmentRepository _repo;
    private readonly IUnitOfWork _uow;

    public CreateAppointmentHandler(IAppointmentRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Result<AppointmentDto>> Handle(
        CreateAppointmentCommand cmd, CancellationToken ct)
    {
        // 1. Validation métier (pas les règles FluentValidation — celles-là sont dans Validator)
        var conflict = await _repo.HasConflictAsync(cmd.VetId, cmd.ScheduledAt, cmd.DurationMinutes, ct);
        if (conflict)
            return Result<AppointmentDto>.Error("This time slot is already booked.");

        // 2. Création via factory Domain
        var result = Appointment.Create(cmd.ClinicId, cmd.VetId, cmd.PatientId, cmd.ScheduledAt, cmd.DurationMinutes);
        if (!result.IsSuccess) return result.Map(_ => (AppointmentDto)null!);

        // 3. Persistence
        await _repo.AddAsync(result.Value, ct);
        await _uow.CommitAsync(ct);

        return Result<AppointmentDto>.Success(result.Value.ToDto());
    }
}
```

## Query — pattern exact

```csharp
// Query = record avec paramètres de filtrage
internal record GetAppointmentsQuery(
    DateTime? From,
    DateTime? To,
    Guid? VetId,
    AppointmentStatus? Status
) : IRequest<Result<IReadOnlyList<AppointmentDto>>>;

// Handler — lecture pure, pas de SaveChanges
internal class GetAppointmentsHandler : IRequestHandler<GetAppointmentsQuery, Result<IReadOnlyList<AppointmentDto>>>
{
    private readonly AgendaDbContext _context;

    public async Task<Result<IReadOnlyList<AppointmentDto>>> Handle(
        GetAppointmentsQuery query, CancellationToken ct)
    {
        var appointments = await _context.Appointments
            .AsNoTracking()
            .WhereIf(query.VetId.HasValue, a => a.VetId == query.VetId)
            .WhereIf(query.From.HasValue, a => a.ScheduledAt >= query.From)
            .WhereIf(query.To.HasValue, a => a.ScheduledAt <= query.To)
            .OrderBy(a => a.ScheduledAt)
            .Select(a => a.ToDto())
            .ToListAsync(ct);

        return Result<IReadOnlyList<AppointmentDto>>.Success(appointments);
    }
}
```

## Behaviors — pipeline MediatR

### ValidationBehavior (incontournable)

```csharp
internal class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var ctx = new ValidationContext<TRequest>(request);
        var failures = _validators
            .SelectMany(v => v.Validate(ctx).Errors)
            .Where(e => e is not null)
            .ToList();

        if (!failures.Any()) return await next();

        var errors = failures.Select(f => new ValidationError(f.PropertyName, f.ErrorMessage));

        // Retourner le type correct selon TResponse
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Invalid(errors);

        // Pour Result<T> — construire via reflection ou generic constraint
        var resultType = typeof(TResponse);
        var method = resultType.GetMethod("Invalid", new[] { typeof(IEnumerable<ValidationError>) });
        return (TResponse)method!.Invoke(null, new object[] { errors })!;
    }
}
```

### LoggingBehavior

```csharp
internal class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}", requestName);

        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        if (sw.ElapsedMilliseconds > 500)
            _logger.LogWarning("Slow handler: {RequestName} took {Elapsed}ms", requestName, sw.ElapsedMilliseconds);

        return response;
    }
}
```

## Domain Events — Notifications MediatR

```csharp
// Contracts — l'event est une interface publique dans les Contracts
// Vetolib.Agenda.Contracts/Events/AppointmentConfirmedEvent.cs
public record AppointmentConfirmedEvent(Guid AppointmentId, Guid ClinicId) : INotification;

// Publication depuis l'entité Domain
public Result Confirm()
{
    if (Status != AppointmentStatus.Pending)
        return Result.Error("Cannot confirm.");
    Status = AppointmentStatus.Confirmed;
    AddDomainEvent(new AppointmentConfirmedEvent(Id, ClinicId));  // BaseEntity.AddDomainEvent
    return Result.Success();
}

// Dans le DbContext — dispatcher les events après SaveChanges
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    var result = await base.SaveChangesAsync(ct);
    await DispatchDomainEventsAsync(ct);
    return result;
}

// Handler dans Billing (écoute les events Agenda via Contracts)
internal class CreateInvoiceOnConfirmationHandler : INotificationHandler<AppointmentConfirmedEvent>
{
    public async Task Handle(AppointmentConfirmedEvent notification, CancellationToken ct)
    {
        // Créer la facture...
    }
}
```

## Enregistrement dans ModuleServiceRegistrar

```csharp
public static IServiceCollection AddAgendaModule(this IServiceCollection services, IConfiguration config)
{
    // MediatR — cet assembly uniquement
    services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(AgendaModuleServiceRegistrar).Assembly);
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    });

    // FluentValidation
    services.AddValidatorsFromAssembly(typeof(AgendaModuleServiceRegistrar).Assembly);

    return services;
}
```

## Conventions strictes

```
✅ Command = opération d'écriture → Result<T>
✅ Query = opération de lecture → Result<IReadOnlyList<T>> ou Result<T>
✅ Handler = 1 use case = 1 handler = 1 fichier
✅ Validator = 1 command/query = 1 validator = 1 fichier
✅ AsNoTracking() sur toutes les queries
✅ Domain Events = publier dans entités, dispatcher dans DbContext

❌ Pas de logique dans les Commands/Queries (ce sont des DTOs)
❌ Pas d'appel direct de repository dans les endpoints (toujours via MediatR)
❌ Pas de handler qui appelle un autre handler directement
❌ Pas de SaveChanges dans les Query handlers
```
