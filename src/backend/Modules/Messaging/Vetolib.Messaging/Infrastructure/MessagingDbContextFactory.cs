using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Infrastructure;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Only used by `dotnet ef migrations add` — never loaded at runtime.
/// </summary>
internal class MessagingDbContextFactory : IDesignTimeDbContextFactory<MessagingDbContext>
{
    public MessagingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseNpgsql("Host=localhost;Database=vetolibdb;Username=postgres;Password=postgres")
            .Options;

        return new MessagingDbContext(options, new MessagingDesignTimeClinicContext(), new MessagingNullPublisher());
    }
}

internal class MessagingDesignTimeClinicContext : IClinicContext
{
    public Guid ClinicId => Guid.Empty;
}

internal class MessagingNullPublisher : IPublisher
{
    public Task Publish(object notification, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
        => Task.CompletedTask;
}
