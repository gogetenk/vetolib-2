using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Infrastructure;

internal class AgendaDbContext : MultiTenantDbContext
{
    public DbSet<Appointment> Appointments => Set<Appointment>();

    public AgendaDbContext(
        DbContextOptions<AgendaDbContext> options,
        IClinicContext clinicContext)
        : base(options, clinicContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // MUST call base first for tenant filter
        builder.ApplyConfigurationsFromAssembly(typeof(AgendaDbContext).Assembly);
    }
}
