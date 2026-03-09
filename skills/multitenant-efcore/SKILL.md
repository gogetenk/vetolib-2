# Skill: Multi-tenancy avec EF Core — Global Query Filter

## Principe

Chaque clinique est un tenant. Toutes les entités "propriété d'une clinique" implémentent
`IMultiTenant`. Le `MultiTenantDbContext` applique automatiquement un filtre `WHERE ClinicId = @current`
sur TOUTES les queries sans exception.

**L'agent n'a JAMAIS à écrire `.Where(x => x.ClinicId == clinicId)` dans les handlers.**

## Shared.Kernel — interfaces fondatrices

```csharp
// Shared.Kernel/IMultiTenant.cs
public interface IMultiTenant
{
    Guid ClinicId { get; }
}

// Shared.Kernel/IClinicContext.cs
// Résolu depuis le JWT token par le middleware d'auth
public interface IClinicContext
{
    Guid ClinicId { get; }
}
```

## Middleware — extraire ClinicId du JWT

```csharp
// Shared.Infrastructure/ClinicContext.cs
internal class ClinicContext : IClinicContext
{
    private readonly IHttpContextAccessor _accessor;

    public ClinicContext(IHttpContextAccessor accessor)
        => _accessor = accessor;

    public Guid ClinicId
    {
        get
        {
            var claim = _accessor.HttpContext?.User.FindFirst("clinic_id");
            if (claim is null || !Guid.TryParse(claim.Value, out var id))
                throw new UnauthorizedAccessException("ClinicId not found in token.");
            return id;
        }
    }
}

// Enregistrement dans chaque ModuleServiceRegistrar
services.AddScoped<IClinicContext, ClinicContext>();
services.AddHttpContextAccessor();
```

## MultiTenantDbContext — filtre automatique

```csharp
// Shared.Infrastructure/MultiTenantDbContext.cs
public abstract class MultiTenantDbContext : DbContext
{
    protected readonly IClinicContext ClinicContext;

    protected MultiTenantDbContext(
        DbContextOptions options,
        IClinicContext clinicContext) : base(options)
    {
        ClinicContext = clinicContext;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(IMultiTenant).IsAssignableFrom(entityType.ClrType))
            {
                ApplyTenantFilter(builder, entityType.ClrType);
            }
        }
    }

    private void ApplyTenantFilter(ModelBuilder builder, Type entityType)
    {
        // Expression: entity => entity.ClinicId == ClinicContext.ClinicId
        var param = Expression.Parameter(entityType, "e");
        var property = Expression.Property(param, nameof(IMultiTenant.ClinicId));
        var clinicIdGetter = Expression.Property(
            Expression.Constant(ClinicContext),
            nameof(IClinicContext.ClinicId));
        var equals = Expression.Equal(property, clinicIdGetter);
        var lambda = Expression.Lambda(equals, param);

        builder.Entity(entityType).HasQueryFilter(lambda);
    }
}
```

## DbContext par module

```csharp
// Agenda/Infrastructure/AgendaDbContext.cs
internal class AgendaDbContext : MultiTenantDbContext
{
    public AgendaDbContext(
        DbContextOptions<AgendaDbContext> options,
        IClinicContext clinicContext)
        : base(options, clinicContext) { }

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Vet> Vets => Set<Vet>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);  // TOUJOURS appeler base en premier
        builder.ApplyConfigurationsFromAssembly(typeof(AgendaDbContext).Assembly);
    }
}
```

## Entités — pattern IMultiTenant

```csharp
// Appointment.cs
internal class Appointment : BaseEntity, IMultiTenant
{
    // IMultiTenant
    public Guid ClinicId { get; private set; }

    // Propriétés métier
    public Guid VetId { get; private set; }
    public Guid PatientId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public int DurationMinutes { get; private set; }
    public AppointmentStatus Status { get; private set; }

    // EF Core navigation (jamais chargé par défaut — explicit Include)
    internal Vet Vet { get; private set; } = null!;
    internal Patient Patient { get; private set; } = null!;

    private Appointment() { }  // Constructeur privé pour EF Core

    public static Result<Appointment> Create(Guid clinicId, Guid vetId, Guid patientId, DateTime scheduledAt, int durationMinutes)
    {
        // ... validation + return
    }
}

// Configuration EF Core
internal class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.ClinicId).IsRequired();
        builder.Property(a => a.Status).HasConversion<string>();
        builder.HasIndex(a => new { a.ClinicId, a.VetId, a.ScheduledAt });
    }
}
```

## Tests multi-tenant — pattern obligatoire

```csharp
// Dans les tests, créer un ClinicContext de test
internal class TestClinicContext : IClinicContext
{
    public Guid ClinicId { get; init; } = Guid.NewGuid();
}

// Test d'isolation
[Fact]
public async Task Clinic_A_Cannot_See_Clinic_B_Appointments()
{
    var clinicA = Guid.NewGuid();
    var clinicB = Guid.NewGuid();

    // Seeder : créer 1 appointment par clinique (bypass filter avec IgnoreQueryFilters)
    using var seedContext = CreateContext(clinicA);
    await seedContext.Appointments.IgnoreQueryFilters()  // OK UNIQUEMENT dans les tests/seeds
        .AddRangeAsync(
            CreateAppointmentFor(clinicA),
            CreateAppointmentFor(clinicB));
    await seedContext.SaveChangesAsync();

    // Query : clinique A ne voit que ses appointments
    using var contextA = CreateContext(clinicA);
    var appointments = await contextA.Appointments.ToListAsync();
    appointments.Should().AllSatisfy(a => a.ClinicId.Should().Be(clinicA));
    appointments.Should().HaveCount(1);
}
```

## Règles absolues

```
✅ Toujours hériter de MultiTenantDbContext pour les DbContext de modules
✅ Toujours appeler base.OnModelCreating(builder) en premier
✅ IMultiTenant sur toutes les entités "appartenant à une clinique"
✅ ClinicId toujours initialisé dans la factory statique de l'entité

❌ INTERDIT : .Where(x => x.ClinicId == ...) dans les handlers (le filter le fait)
❌ INTERDIT : .IgnoreQueryFilters() dans le code de production
❌ INTERDIT : Partager un DbContext entre modules
❌ INTERDIT : Exposer ClinicId dans les DTOs de réponse (c'est interne)
```

## Migrations — une par module

```bash
# Chaque module gère ses propres migrations
cd src/Modules/Agenda/Vetolib.Agenda
dotnet ef migrations add InitialAgendaSchema \
    --context AgendaDbContext \
    --output-dir Infrastructure/Migrations \
    --project . \
    --startup-project ../../../../Vetolib.Api

dotnet ef database update --context AgendaDbContext --startup-project ../../../../Vetolib.Api
```

Les migrations de différents modules peuvent être appliquées dans n'importe quel ordre
car ils n'ont pas de FK cross-modules (uniquement des IDs copiés, pas des FK EF Core).
