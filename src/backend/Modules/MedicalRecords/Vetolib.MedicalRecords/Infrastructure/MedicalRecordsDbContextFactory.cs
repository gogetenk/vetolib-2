using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Infrastructure;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Only used by `dotnet ef migrations add` — never loaded at runtime.
/// </summary>
internal class MedicalRecordsDbContextFactory : IDesignTimeDbContextFactory<MedicalRecordsDbContext>
{
    public MedicalRecordsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseNpgsql("Host=localhost;Database=vetolibdb;Username=postgres;Password=postgres")
            .Options;

        return new MedicalRecordsDbContext(options, new DesignTimeClinicContext(), new NullPublisher());
    }
}

internal class DesignTimeClinicContext : IClinicContext
{
    public Guid ClinicId => Guid.Empty;
}

internal class NullPublisher : IPublisher
{
    public Task Publish(object notification, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
        => Task.CompletedTask;
}
