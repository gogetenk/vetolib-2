using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Breeding.Api;
using Vetolib.Breeding.Infrastructure;
using Vetolib.Shared.Infrastructure.Behaviors;

namespace Vetolib.Breeding;

public static class ModuleServiceRegistrar
{
    public static IServiceCollection AddBreedingModule(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ModuleServiceRegistrar).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(ModuleServiceRegistrar).Assembly, includeInternalTypes: true);

        return services;
    }

    /// <summary>
    /// Register BreedingDbContext with a connection string (for non-Aspire scenarios / tests).
    /// When using Aspire, call builder.AddNpgsqlDbContext&lt;BreedingDbContext&gt;("vetolibdb") instead.
    /// </summary>
    public static IServiceCollection AddBreedingDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BreedingDbContext>(opts =>
            opts.UseNpgsql(connectionString));
        return services;
    }

    public static IEndpointRouteBuilder MapBreedingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHeatCycleEndpoints();
        return app;
    }
}
