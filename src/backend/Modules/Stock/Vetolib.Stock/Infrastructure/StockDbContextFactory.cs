using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Vetolib.Shared.Kernel;

namespace Vetolib.Stock.Infrastructure;

/// <summary>
/// Design-time factory for EF Core migrations.
/// Only used by `dotnet ef migrations add` — never loaded at runtime.
/// </summary>
internal class StockDbContextFactory : IDesignTimeDbContextFactory<StockDbContext>
{
    public StockDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseNpgsql("Host=localhost;Database=vetolibdb;Username=postgres;Password=postgres")
            .Options;

        return new StockDbContext(options, new DesignTimeStockClinicContext(), new StockNullPublisher());
    }
}

internal class DesignTimeStockClinicContext : IClinicContext
{
    public Guid ClinicId => Guid.Empty;
}

internal class StockNullPublisher : IPublisher
{
    public Task Publish(object notification, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
        => Task.CompletedTask;
}
