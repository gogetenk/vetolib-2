using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetolib.Preferences.Application.Services;
using Vetolib.Preferences.Contracts;
using Vetolib.Preferences.Infrastructure;
using Vetolib.Shared.Infrastructure.Behaviors;

namespace Vetolib.Preferences;

public static class ModuleServiceRegistrar
{
    public static IServiceCollection AddPreferencesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // MediatR — register handlers from this assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ModuleServiceRegistrar).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(ModuleServiceRegistrar).Assembly, includeInternalTypes: true);

        // IMemoryCache — used by PreferenceChecker for 5-minute TTL caching
        services.AddMemoryCache();

        // IPreferenceChecker — cross-module interface implemented here
        services.AddScoped<IPreferenceChecker, PreferenceChecker>();

        // Note: PreferenceChangedConsumer for cache invalidation must be registered in
        // the host via x.AddConsumers(typeof(ModuleServiceRegistrar).Assembly) in Program.cs.
        // Current fallback: cache entries expire after 5 minutes (CacheTtl).

        return services;
    }

    /// <summary>
    /// Register PreferencesDbContext with a connection string (for non-Aspire scenarios / tests).
    /// When using Aspire, call builder.AddNpgsqlDbContext&lt;PreferencesDbContext&gt;("vetolibdb") instead.
    /// </summary>
    public static IServiceCollection AddPreferencesDbContext(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<PreferencesDbContext>(opts =>
            opts.UseNpgsql(connectionString));
        return services;
    }

    public static IEndpointRouteBuilder MapPreferencesEndpoints(this IEndpointRouteBuilder app)
    {
        // No endpoints in this scaffold task — they will be added in subsequent tasks.
        return app;
    }
}
