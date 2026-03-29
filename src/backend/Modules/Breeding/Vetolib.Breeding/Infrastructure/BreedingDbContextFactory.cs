using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Vetolib.Shared.Kernel;

namespace Vetolib.Breeding.Infrastructure;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Only used by `dotnet ef migrations add` -- never loaded at runtime.
/// </summary>
internal class BreedingDbContextFactory : IDesignTimeDbContextFactory<BreedingDbContext>
{
    public BreedingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BreedingDbContext>()
            .UseNpgsql(Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? "Host=localhost;Database=vetolibdb;Username=postgres;Password=postgres")
            .Options;

        return new BreedingDbContext(options, new DesignTimeBreedingClinicContext(), new BreedingNullPublisher());
    }
}

internal class DesignTimeBreedingClinicContext : IClinicContext
{
    public Guid ClinicId => Guid.Empty;
}

internal class BreedingNullPublisher : IPublisher
{
    public Task Publish(object notification, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
        => Task.CompletedTask;
}
