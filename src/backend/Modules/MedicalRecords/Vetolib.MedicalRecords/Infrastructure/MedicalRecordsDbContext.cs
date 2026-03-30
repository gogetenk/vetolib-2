using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class MedicalRecordsDbContext : MultiTenantDbContext
{
    private readonly IClinicContext _clinicContext;

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<PatientOwner> PatientOwners => Set<PatientOwner>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<DrugCatalogEntry> DrugCatalogEntries => Set<DrugCatalogEntry>();
    public DbSet<WeightEntry> WeightEntries => Set<WeightEntry>();
    public DbSet<MedicalRecordTemplate> MedicalRecordTemplates => Set<MedicalRecordTemplate>();

    public MedicalRecordsDbContext(
        DbContextOptions<MedicalRecordsDbContext> options,
        IClinicContext clinicContext,
        IPublisher publisher)
        : base(options, clinicContext, publisher)
    {
        _clinicContext = clinicContext;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // MUST call base first for tenant filter
        builder.ApplyConfigurationsFromAssembly(typeof(MedicalRecordsDbContext).Assembly);

        // DrugCatalogEntry has nullable ClinicId (null = global, non-null = clinic-specific).
        // MultiTenantDbContext only filters IMultiTenant entities, so we add a custom filter here:
        // WHERE ClinicId IS NULL OR ClinicId = @currentClinicId
        builder.Entity<DrugCatalogEntry>()
            .HasQueryFilter(d => d.ClinicId == null || d.ClinicId == _clinicContext.ClinicId);
    }
}
