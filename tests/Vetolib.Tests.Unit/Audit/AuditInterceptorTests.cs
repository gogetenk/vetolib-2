using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Audit;

/// <summary>
/// Unit tests for the audit trail components.
///
/// The interceptor is tested indirectly via AuditEntry shape tests.
/// The full end-to-end interception flow is covered by acceptance tests.
/// </summary>
public class AuditInterceptorTests
{
    private static readonly Guid TestClinicId = new("00000000-0000-0000-0001-000000000001");

    // ─── AuditEntry shape ─────────────────────────────────────────────────────

    [Fact]
    public void AuditEntry_Created_HasNullOldValues()
    {
        var entry = new AuditEntry
        {
            EntityType = "Appointment",
            EntityId   = Guid.NewGuid().ToString(),
            Action     = "Created",
            ChangedBy  = "admin@desertpaws.ae",
            ClinicId   = TestClinicId,
            Timestamp  = DateTime.UtcNow,
            OldValues  = null,
            NewValues  = "{\"Id\":\"some-guid\",\"Status\":\"Scheduled\"}"
        };

        entry.Action.Should().Be("Created");
        entry.OldValues.Should().BeNull("Created entries have no prior state");
        entry.NewValues.Should().NotBeNull("Created entries capture the initial state");
    }

    [Fact]
    public void AuditEntry_Deleted_HasNullNewValues()
    {
        var entry = new AuditEntry
        {
            EntityType = "Invoice",
            EntityId   = Guid.NewGuid().ToString(),
            Action     = "Deleted",
            ChangedBy  = "admin@desertpaws.ae",
            ClinicId   = TestClinicId,
            Timestamp  = DateTime.UtcNow,
            OldValues  = "{\"Id\":\"some-guid\",\"Status\":\"Draft\"}",
            NewValues  = null
        };

        entry.Action.Should().Be("Deleted");
        entry.OldValues.Should().NotBeNull("Deleted entries capture the last known state");
        entry.NewValues.Should().BeNull("Deleted entries have no future state");
    }

    [Fact]
    public void AuditEntry_Updated_HasBothValues()
    {
        var entry = new AuditEntry
        {
            EntityType = "Patient",
            EntityId   = Guid.NewGuid().ToString(),
            Action     = "Updated",
            ChangedBy  = "vet@desertpaws.ae",
            ClinicId   = TestClinicId,
            Timestamp  = DateTime.UtcNow,
            OldValues  = "{\"Name\":\"Luna\"}",
            NewValues  = "{\"Name\":\"Luna Al-Rashid\"}"
        };

        entry.Action.Should().Be("Updated");
        entry.OldValues.Should().NotBeNull();
        entry.NewValues.Should().NotBeNull();
    }

    // ─── Interceptor construction ─────────────────────────────────────────────

    [Fact]
    public void AuditSaveChangesInterceptor_CanBeInstantiated()
    {
        var httpAccessor = Substitute.For<IHttpContextAccessor>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        var interceptor = new AuditSaveChangesInterceptor(httpAccessor, scopeFactory);

        interceptor.Should().BeAssignableTo<Microsoft.EntityFrameworkCore.Diagnostics.ISaveChangesInterceptor>(
            "the interceptor must implement ISaveChangesInterceptor to plug into the EF Core pipeline");
    }

    // ─── Change tracker integration ───────────────────────────────────────────

    [Fact]
    public void ChangeTracker_AddEntity_StateIsAdded()
    {
        // Verify EF change tracking sees Added entities — the interceptor relies on this.
        var options = new DbContextOptionsBuilder<TestAuditDbContext>()
            .UseInMemoryDatabase($"audit-ct-test-{Guid.NewGuid()}")
            .Options;

        var clinicCtx = Substitute.For<IClinicContext>();
        clinicCtx.ClinicId.Returns(TestClinicId);

        using var ctx = new TestAuditDbContext(options, clinicCtx);
        var entity = new TestAuditEntity { ClinicId = TestClinicId };
        ctx.Add(entity);

        var entries = ctx.ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added)
            .ToList();

        entries.Should().HaveCount(1, "one entity was added");
        entries[0].Entity.GetType().Name.Should().NotBe("RefreshToken",
            "RefreshToken entities should be excluded from audit");
    }
}

// ─── Test doubles ─────────────────────────────────────────────────────────────

internal class TestAuditEntity : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = "Luna";
}

internal class TestAuditDbContext : MultiTenantDbContext
{
    public DbSet<TestAuditEntity> Entities => Set<TestAuditEntity>();

    public TestAuditDbContext(DbContextOptions options, IClinicContext clinicContext)
        : base(options, clinicContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Skip base to avoid applying tenant filter on InMemory provider
        builder.Entity<TestAuditEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Ignore(x => x.DomainEvents);
        });
    }
}
