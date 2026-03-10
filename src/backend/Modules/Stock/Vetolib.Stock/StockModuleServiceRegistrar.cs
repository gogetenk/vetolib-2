using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Shared.Infrastructure.Behaviors;
using Vetolib.Stock.Api;
using Vetolib.Stock.Infrastructure;

namespace Vetolib.Stock;

public static class StockModuleServiceRegistrar
{
    public static IServiceCollection AddStockModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(StockModuleServiceRegistrar).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(StockModuleServiceRegistrar).Assembly, includeInternalTypes: true);

        return services;
    }

    public static IServiceCollection AddStockDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<StockDbContext>(opts =>
            opts.UseNpgsql(connectionString));
        return services;
    }

    public static IEndpointRouteBuilder MapStockEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapStockApiEndpoints();
        return app;
    }
}
