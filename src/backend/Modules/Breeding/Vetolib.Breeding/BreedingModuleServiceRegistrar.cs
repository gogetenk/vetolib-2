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

public static class BreedingModuleServiceRegistrar
{
    public static IServiceCollection AddBreedingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(BreedingModuleServiceRegistrar).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(BreedingModuleServiceRegistrar).Assembly, includeInternalTypes: true);

        return services;
    }

    public static IServiceCollection AddBreedingDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BreedingDbContext>(opts =>
            opts.UseNpgsql(connectionString));
        return services;
    }

    public static IEndpointRouteBuilder MapBreedingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapBreedingApiEndpoints();
        app.MapLitterEndpoints();
        app.MapPregnancyApiEndpoints();
        return app;
    }
}
