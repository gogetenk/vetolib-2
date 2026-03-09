using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Infrastructure;

internal class MedicalRecordsDbContext : MultiTenantDbContext
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<PatientOwner> PatientOwners => Set<PatientOwner>();

    public MedicalRecordsDbContext(
        DbContextOptions<MedicalRecordsDbContext> options,
        IClinicContext clinicContext)
        : base(options, clinicContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // MUST call base first for tenant filter
        builder.ApplyConfigurationsFromAssembly(typeof(MedicalRecordsDbContext).Assembly);
    }
}
