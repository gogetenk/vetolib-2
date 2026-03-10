using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Vetolib.Shared.Infrastructure;

/// <summary>
/// Dedicated DbContext for the shared.audit_log table.
/// Separate from module-specific contexts so that the AuditSaveChangesInterceptor
/// can persist audit entries after a module's SaveChanges without re-entering
/// the same context transaction.
///
/// This context is registered via AddAuditDbContext() and receives its connection
/// string from the Aspire host (same "vetolibdb" named connection).
/// </summary>
public class AuditDbContext : DbContext
{
    public DbSet<AuditEntry> AuditLog => Set<AuditEntry>();

    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AuditEntry>(e =>
        {
            e.ToTable("audit_log", "shared");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).UseIdentityByDefaultColumn();
            e.Property(a => a.EntityType).HasMaxLength(200).IsRequired();
            e.Property(a => a.EntityId).HasMaxLength(50).IsRequired();
            e.Property(a => a.Action).HasMaxLength(20).IsRequired();
            e.Property(a => a.ChangedBy).HasMaxLength(256);
            e.Property(a => a.OldValues).HasColumnType("text");
            e.Property(a => a.NewValues).HasColumnType("text");
            e.HasIndex(a => new { a.ClinicId, a.Timestamp });
            e.HasIndex(a => new { a.EntityType, a.EntityId });
        });
    }
}

/// <summary>Design-time factory for EF Core migrations on AuditDbContext.</summary>
public class AuditDbContextFactory : IDesignTimeDbContextFactory<AuditDbContext>
{
    public AuditDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseNpgsql("Host=localhost;Database=vetolibdb;Username=postgres;Password=postgres")
            .Options;

        return new AuditDbContext(options);
    }
}
