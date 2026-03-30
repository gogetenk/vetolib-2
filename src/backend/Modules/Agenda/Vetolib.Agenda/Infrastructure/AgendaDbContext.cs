using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Agenda.Application.Domain;
using Vetolib.Shared.Infrastructure;
using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Infrastructure;

internal class AgendaDbContext : MultiTenantDbContext
{
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ConsultationType> ConsultationTypes => Set<ConsultationType>();
    public DbSet<WaitlistEntry> WaitlistEntries => Set<WaitlistEntry>();

    public AgendaDbContext(
        DbContextOptions<AgendaDbContext> options,
        IClinicContext clinicContext,
        IPublisher publisher)
        : base(options, clinicContext, publisher)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // MUST call base first for tenant filter
        builder.ApplyConfigurationsFromAssembly(typeof(AgendaDbContext).Assembly);

        // Register MassTransit outbox tables in the "agenda" schema
        builder.AddInboxStateEntity(b => b.ToTable("inbox_state", "agenda"));
        builder.AddOutboxMessageEntity(b => b.ToTable("outbox_message", "agenda"));
        builder.AddOutboxStateEntity(b => b.ToTable("outbox_state", "agenda"));
    }
}
